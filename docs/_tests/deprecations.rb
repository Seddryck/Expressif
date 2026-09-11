require 'jekyll'
require 'json'
require 'tmpdir'
require 'fileutils'

# Render the production templates through Jekyll, then exercise the page validator.
root = File.expand_path('../..', __dir__)
catalog = File.join(root, 'docs', '_data')
source_rules = JSON.parse(File.read(File.join(catalog, 'usage-lifecycle.json')))
expected_ids = %w[implicit-tuple-binding-adjacent implicit-tuple-binding-chunk-while implicit-tuple-binding-map-over implicit-tuple-binding-map-with]
raise 'Unexpected lifecycle rule identities' unless source_rules.map { |rule| rule['Id'] }.sort == expected_ids.sort

%w[actual empty callable-only usage-only escaping].each do |scenario|
  Dir.mktmpdir('expressif-deprecations-') do |directory|
    source = File.join(directory, 'source')
    data = File.join(source, '_data')
    destination = File.join(directory, 'site')
    FileUtils.mkdir_p(data)
    FileUtils.cp(File.join(root, 'docs', 'deprecations.md'), source)
    FileUtils.cp_r(File.join(root, 'docs', '_includes'), source)
    %w[function predicate accumulator usage-lifecycle].each do |name|
      FileUtils.cp(File.join(catalog, "#{name}.json"), data)
    end
    if %w[empty usage-only escaping].include?(scenario)
      %w[function predicate accumulator].each { |kind| File.write(File.join(data, "#{kind}.json"), '[]') }
    end
    rules = Marshal.load(Marshal.dump(source_rules))
    rules = [] if %w[empty callable-only].include?(scenario)
    if scenario == 'callable-only'
      File.write(File.join(data, 'function.json'), JSON.generate([
        { 'Name' => 'old', 'Scope' => 'array', 'IsPublic' => true, 'Deprecated' => true, 'Replacement' => 'new', 'Sunset' => nil },
        { 'Name' => 'new', 'Scope' => 'array', 'IsPublic' => true }
      ]))
    end
    if scenario == 'escaping'
      rules = [rules.first]
      rules.first['DeprecatedSince'] = '2.0'
      rules.first['Sunset'] = nil
      rules.first['Examples'] = [{
        'Deprecated' => 'adjacent(f("<old>&"))',
        'Replacement' => 'adjacent(~f | g("<new>&", ($1 | h($0))))',
        'Availability' => 'planned', 'AppliesWhen' => 'Only when f accepts "<new>&".', 'Expected' => '<result>&'
      }]
    end
    File.write(File.join(data, 'usage-lifecycle.json'), JSON.generate(rules))
    Jekyll::Site.new(Jekyll.configuration({
      'source' => source, 'destination' => destination, 'quiet' => true,
      'baseurl' => '/Expressif', 'exclude' => [], 'plugins' => []
    })).process
    command = ['pwsh', '-NoProfile', '-File', File.join(root, 'Test-LanguageDeprecationPage.ps1'), '-CatalogPath', data, '-SitePath', destination]
    raise "Page validation failed: #{scenario}" unless system(*command)
    page_path = File.join(destination, 'deprecations', 'index.html')
    page = File.read(page_path)
    if scenario == 'actual'
      rules.each do |rule|
        raise 'Broken operator link' unless File.file?(File.join(root, 'docs', rule['OperatorPath'].delete_suffix('/') + '.md'))
        raise 'Missing baseurl in operator link' unless page.include?("/Expressif#{rule['OperatorPath']}")
      end
      # A truncated expression must fail, not silently pass the validator.
      File.write(page_path, page.sub('data-replacement="{1, 2, 5} | adjacent(~subtract)"', 'data-replacement="adjacent"'))
      raise 'Validator accepted a truncated replacement' if system(*command, out: File::NULL, err: File::NULL)
    end
    puts "Validated Jekyll scenario: #{scenario}"
  end
end
