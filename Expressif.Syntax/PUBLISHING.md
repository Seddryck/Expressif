# Publishing the VS Code extension

Visual Studio Marketplace publication is intentionally manual. CI generates, validates, and retains the versioned VSIX, but it does not publish the extension to the Marketplace.

## Publish a release

1. Wait for the normal Expressif release workflow to complete for the intended version.
2. Download `expressif-syntax-<version>-vscode.vsix` from that release's `syntax-assets` artifact or GitHub release assets.
3. Verify the downloaded package:

   ```powershell
   ./Test-VsCodeExtension.ps1 -PackagePath <path-to-vsix>
   ```

4. Confirm that the package version and release version are identical.
5. Authenticate `vsce` locally for the `seddryck` Marketplace publisher. Keep the credential outside the repository and GitHub Actions.
6. Publish the exact validated package:

   ```powershell
   npx --yes @vscode/vsce@3.9.2 publish --packagePath <path-to-vsix>
   ```

7. Confirm that `seddryck.expressif-syntax-highlighting` shows the published version in the Visual Studio Marketplace.

Do not add a Marketplace publishing credential or automatic publication job to CI. Future OIDC trusted publishing is tracked in [issue #1082](https://github.com/Seddryck/Expressif/issues/1082).
