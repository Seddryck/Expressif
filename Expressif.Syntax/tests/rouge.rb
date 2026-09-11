require 'json'

require_relative ARGV.fetch(0, '../obj/verification/expressif.rb')

class TokenizationError < StandardError; end

samples = JSON.parse(File.read(File.join(__dir__, 'samples.json')))
samples.each do |sample|
  tokens = Rouge::Lexers::Expressif.new.lex(sample.fetch('source')).to_a
  tildes = tokens.select { |token, value| token == Rouge::Token['Operator'] && value.include?('~') }.sum { |_, value| value.count('~') }
  expected = sample['quoted'] ? 0 : sample['source'].count('~')
  raise TokenizationError, "Tilde classification: #{sample['source']}" unless tildes == expected
  if sample['quoted'] && !tokens.any? { |token, value| token.qualname.start_with?('Literal.String') && value.include?('~') }
    raise TokenizationError, "String classification"
  end
  (sample['operators'] || []).each do |operator|
    raise TokenizationError, "Operator #{operator}: #{sample['source']}" unless tokens.any? { |token, value| token == Rouge::Token['Operator'] && value == operator }
  end
  sample['names'].each do |name|
    raise TokenizationError, "Callable #{name}: #{sample['source']}" unless tokens.any? { |token, value| token == Rouge::Token['Name.Function'] && value == name }
  end
end
puts "Rouge: #{samples.length} tokenization cases passed"
