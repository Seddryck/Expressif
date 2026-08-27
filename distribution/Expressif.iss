#define AppName "Expressif"
#define AppPublisher "Seddryck"
#define AppExeName "expressif.exe"

#ifndef AppVersion
  #define AppVersion "0.0.0-local"
#endif

#ifndef RuntimeIdentifier
    #define RuntimeIdentifier "win-x64"
#endif

#ifndef TargetFramework
    #define TargetFramework "net10.0"
#endif

#ifndef TargetArchitecture
    #define TargetArchitecture "x64compatible"
#endif

#ifndef PublishDirectory
  #define PublishDirectory ".\bin\{#TargetFramework}\{#RuntimeIdentifier}"
#endif

#ifndef OutputDirectory
  #define OutputDirectory ".\bin\"
#endif

#ifndef OutputBaseFilename
  #define OutputBaseFilename "expressif-" + AppVersion + "-" + TargetFramework + "-" + RuntimeIdentifier + "-setup"
#endif

[Setup]
AppId={{9F6D1A4C-5E73-4A91-BB8F-2F3D7C8E6A55}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/Seddryck/{#AppName}
AppSupportURL=https://github.com/Seddryck/{#AppName}/issues
AppUpdatesURL=https://github.com/Seddryck/{#AppName}/releases
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir={#OutputDirectory}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed={#TargetArchitecture}
ArchitecturesInstallIn64BitMode={#TargetArchitecture}
PrivilegesRequired=admin
ChangesEnvironment=yes
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "{#PublishDirectory}\{#BuildIdentity}.exe"; \
  DestDir: "{app}"; \
  DestName: "{#CommandName}.exe"; \
  Flags: ignoreversion
Source: "{#PublishDirectory}\*"; \
  DestDir: "{app}"; \
  Excludes: "{#BuildIdentity}.exe"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName} Command Prompt"; Filename: "{cmd}"; Parameters: "/K cd /d ""{app}"""

[Code]

const
  EnvironmentKey =
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';
  RequiredDotNetMajorVersion = 10;

function NormalizePathEntry(Value: string): string;
begin
  Result := RemoveBackslashUnlessRoot(Trim(Value));
end;

function PathContains(CurrentPath, Entry: string): Boolean;
var
  NormalizedPath: string;
  NormalizedEntry: string;
begin
  NormalizedPath := Uppercase(CurrentPath);
  NormalizedEntry := Uppercase(NormalizePathEntry(Entry));

  StringChangeEx(NormalizedPath, '/', '\', True);
  StringChangeEx(NormalizedEntry, '/', '\', True);

  Result :=
    Pos(
      ';' + NormalizedEntry + ';',
      ';' + NormalizedPath + ';'
    ) > 0;
end;

procedure AddToSystemPath(Entry: string);
var
  CurrentPath: string;
  NormalizedEntry: string;
begin
  NormalizedEntry := NormalizePathEntry(Entry);

  if not RegQueryStringValue(
    HKEY_LOCAL_MACHINE,
    EnvironmentKey,
    'Path',
    CurrentPath
  ) then
    CurrentPath := '';

  if PathContains(CurrentPath, NormalizedEntry) then
    Exit;

  if (CurrentPath <> '') and
     (CurrentPath[Length(CurrentPath)] <> ';') then
    CurrentPath := CurrentPath + ';';

  RegWriteExpandStringValue(
    HKEY_LOCAL_MACHINE,
    EnvironmentKey,
    'Path',
    CurrentPath + NormalizedEntry
  );
end;

procedure RemoveFromSystemPath(Entry: string);
var
  CurrentPath: string;
  RemainingPath: string;
  UpdatedPath: string;
  CurrentEntry: string;
  NormalizedEntry: string;
  SeparatorPosition: Integer;
begin
  if not RegQueryStringValue(
    HKEY_LOCAL_MACHINE,
    EnvironmentKey,
    'Path',
    CurrentPath
  ) then
    Exit;

  NormalizedEntry := Uppercase(NormalizePathEntry(Entry));
  RemainingPath := CurrentPath;
  UpdatedPath := '';

  while RemainingPath <> '' do
  begin
    SeparatorPosition := Pos(';', RemainingPath);

    if SeparatorPosition > 0 then
    begin
      CurrentEntry := Copy(
        RemainingPath,
        1,
        SeparatorPosition - 1
      );

      Delete(
        RemainingPath,
        1,
        SeparatorPosition
      );
    end
    else
    begin
      CurrentEntry := RemainingPath;
      RemainingPath := '';
    end;

    CurrentEntry := Trim(CurrentEntry);

    if (CurrentEntry <> '') and
       (Uppercase(NormalizePathEntry(CurrentEntry)) <> NormalizedEntry) then
    begin
      if UpdatedPath <> '' then
        UpdatedPath := UpdatedPath + ';';

      UpdatedPath := UpdatedPath + CurrentEntry;
    end;
  end;

  RegWriteExpandStringValue(
    HKEY_LOCAL_MACHINE,
    EnvironmentKey,
    'Path',
    UpdatedPath
  );
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    AddToSystemPath(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RemoveFromSystemPath(ExpandConstant('{app}'));
end;



function GetDotNetArchitecture(): string;
begin
  if '{#RuntimeIdentifier}' = 'win-arm64' then
    Result := 'arm64'
  else
    Result := 'x64';
end;

function HasRequiredDotNetRuntime(): Boolean;
var
  RuntimeKey: string;
  RuntimeVersions: TArrayOfString;
  VersionPrefix: string;
  I: Integer;
begin
  Result := False;

  RuntimeKey :=
    'SOFTWARE\dotnet\Setup\InstalledVersions\' +
    GetDotNetArchitecture() +
    '\sharedfx\Microsoft.NETCore.App';

  if not RegGetValueNames(
    HKEY_LOCAL_MACHINE_32,
    RuntimeKey,
    RuntimeVersions
  ) then
    Exit;

  VersionPrefix := IntToStr(RequiredDotNetMajorVersion) + '.';

  for I := 0 to GetArrayLength(RuntimeVersions) - 1 do
  begin
    if Pos(VersionPrefix, RuntimeVersions[I]) = 1 then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function InitializeSetup(): Boolean;
var
  Architecture: string;
begin
  Result := HasRequiredDotNetRuntime();

  if Result then
    Exit;

  Architecture := Uppercase(GetDotNetArchitecture());

  MsgBox(
    '{#AppName} requires the Microsoft .NET ' +
    IntToStr(RequiredDotNetMajorVersion) +
    ' Runtime (' + Architecture + ').' + #13#10 + #13#10 +
    'Install the required runtime and then run this installer again.' + #13#10 + #13#10 +
    'Required framework: Microsoft.NETCore.App ' +
    IntToStr(RequiredDotNetMajorVersion) + '.x',
    mbError,
    MB_OK
  );
end;
