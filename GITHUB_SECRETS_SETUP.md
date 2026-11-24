# GitHub Secrets Setup for CI/CD Build

## Required Secret: UNITY_LICENSE

**Copy the entire content below** (including `<?xml ... </root>`) and add it as a GitHub secret named `UNITY_LICENSE`:

---

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <License Id="Terms" IssueDate="2026-11-22T00:01:53.030Z" Version="1.0">
    <EntitlementGroups>
      <EntitlementGroup Id="20067689545194-UnityPersonal" Namespace="unity_editor" Product="Unity Personal" SubsGroup="Unity Editor" Tag="UnityPersonal" Type="EDITOR">
        <Context>
          <User Id="20067689545194" />
          <StartDate>2026-11-19T22:38:14Z</StartDate>
          <UpdateDate>2026-12-22T00:01:53.030Z</UpdateDate>
          <Identifiers>
            <Identifier Id="6430c1da9ccd4aad9a773a965aa1cf20" Type="Legacy.MachineBinding1" />
            <Identifier Id="6430c1da9ccd4aad9a773a965aa1cf20" Type="Legacy.MachineBinding2" />
          </Identifiers>
        </Context>
        <Entitlements>
          <Entitlement Id="com.unity.editor" />
          <Entitlement Id="com.unity.editor.ui" />
          <Entitlement Id="com.unity.editor.headless" />
          <Entitlement Id="com.unity.editor.dark-skin" />
          <Entitlement Id="com.unity.editor.legacy.team-license" />
          <Entitlement Id="com.unity.editor.platforms.android" />
          <Entitlement Id="com.unity.editor.platforms.ios" />
          <Entitlement Id="com.unity.editor.platforms.uwp" />
        </Entitlements>
      </EntitlementGroup>
      <EntitlementGroup Id="organization-all-asset-store-upm-packages" Namespace="asset_store" Product="Asset Store" Type="upm-package">
        <Context>
          <User Id="20067689545194" />
          <UpdateDate>2026-12-22T00:01:53.030Z</UpdateDate>
          <Identifiers>
            <Identifier Id="6430c1da9ccd4aad9a773a965aa1cf20" Type="Legacy.MachineBinding1" />
            <Identifier Id="6430c1da9ccd4aad9a773a965aa1cf20" Type="Legacy.MachineBinding2" />
          </Identifiers>
        </Context>
        <Entitlements>
          <Entitlement Id="com.meta.xr.mrutilitykit" />
          <Entitlement Id="com.meta.xr.runtimeoptimizer" />
          <Entitlement Id="com.meta.xr.sdk.all" />
          <Entitlement Id="com.meta.xr.sdk.audio" />
          <Entitlement Id="com.meta.xr.sdk.avatars" />
          <Entitlement Id="com.meta.xr.sdk.avatars.sample.assets" />
          <Entitlement Id="com.meta.xr.sdk.core" />
          <Entitlement Id="com.meta.xr.sdk.haptics" />
          <Entitlement Id="com.meta.xr.sdk.interaction" />
          <Entitlement Id="com.meta.xr.sdk.interaction.ovr" />
          <Entitlement Id="com.meta.xr.sdk.interaction.ovr.samples" />
          <Entitlement Id="com.meta.xr.sdk.platform" />
          <Entitlement Id="com.meta.xr.sdk.sharedassets" />
          <Entitlement Id="com.meta.xr.sdk.utilities" />
          <Entitlement Id="com.meta.xr.sdk.voice" />
          <Entitlement Id="com.meta.xr.sdk.voice.composer" />
          <Entitlement Id="com.meta.xr.sdk.voice.dictation" />
          <Entitlement Id="com.meta.xr.sdk.voice.telemetry" />
          <Entitlement Id="com.meta.xr.simulator" />
          <Entitlement Id="com.meta.xr.simulator.samples" />
          <Entitlement Id="com.meta.xr.simulator.synthenvbuilder" />
          <Entitlement Id="com.microsoft.mixedreality.openxr" />
          <Entitlement Id="com.microsoft.mixedreality.webview.unity" />
          <Entitlement Id="com.microsoft.mrtk.graphicstools.unity" />
          <Entitlement Id="com.microsoft.mrtk.tts.windows" />
          <Entitlement Id="com.microsoft.spatialaudio.spatializer.unity" />
          <Entitlement Id="org.mixedrealitytoolkit.audio" />
          <Entitlement Id="org.mixedrealitytoolkit.core" />
          <Entitlement Id="org.mixedrealitytoolkit.diagnostics" />
          <Entitlement Id="org.mixedrealitytoolkit.extendedassets" />
          <Entitlement Id="org.mixedrealitytoolkit.input" />
          <Entitlement Id="org.mixedrealitytoolkit.spatialmanipulation" />
          <Entitlement Id="org.mixedrealitytoolkit.standardassets" />
          <Entitlement Id="org.mixedrealitytoolkit.tools" />
          <Entitlement Id="org.mixedrealitytoolkit.uxcomponents" />
          <Entitlement Id="org.mixedrealitytoolkit.uxcomponents.noncanvas" />
          <Entitlement Id="org.mixedrealitytoolkit.uxcore" />
          <Entitlement Id="org.mixedrealitytoolkit.windowsspeech" />
        </Entitlements>
      </EntitlementGroup>
    </EntitlementGroups>
  </License>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <SignedInfo>
      <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments" />
      <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" />
      <Reference URI="#Terms">
        <Transforms>
          <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
        </Transforms>
        <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256" />
        <DigestValue>YWf7+w4iKDoIuFkRSEF8FJDCrB33DgRnuBonl+5oOKE=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>c/dEtweitt8ovTRUaJxe2tl/Qyejwo7zeXYo4opzJwySAdPoaM2YZtuPPmDTrzzUEZ+4D355Z7ZB
Azn2tn9K3QQUw0j8bk0eIPS7IdycfDfaLPpJEV6OS7lKDMpytSusX+ZdhohBfqbmZ0ddx5cB6lM2
o6EqsFQYxjbYhJCAy7RLD4iprslteLRyT+DAZnPAVYUQQagDWdLSWxMYC0ehK0AG5Fr/WyZL+f8C
dJ12LvuH1Qg2BYTSusTq7Hw0SvHumtkwkAnKliDJQcE5DoGecA5lZDhHV5Dcdntq5lr9oz7IYU1v
WQsRVxtN9dUtbbirMUWCZZ6kRAgKcj3LEL2clg==</SignatureValue>
  </Signature>
</root>
```

---

## How to Add the Secret

1. Go to your GitHub repository: https://github.com/jonnythebot/JohnWickArsenal
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. **Name**: `UNITY_LICENSE`
5. **Secret**: Paste the entire XML content above (including the `<?xml` declaration and `</root>`)
6. Click **Add secret**

## Alternative: Username/Password (if XML doesn't work)

If the XML license fails (machine-binding issues), add these instead:

1. **UNITY_USERNAME**: `jm28lockwood@yahoo.com`
2. **UNITY_PASSWORD**: `[your Unity account password - must NOT have 2FA enabled]`
3. **UNITY_SERIAL**: (optional, for Pro/Enterprise)

## Triggering the Build

After adding the secret, trigger a build:

1. Go to **Actions** tab in GitHub
2. Select **Build Android Mod** workflow
3. Click **Run workflow**
4. Choose branch `master` and click **Run workflow**

## Expected Outcome

The workflow will:
1. ✅ Setup dependencies (.NET, Android SDK, Unity Docker)
2. ✅ Prepare Unity license (using XML from UNITY_LICENSE secret)
3. ✅ Sync source code to Unity project
4. ✅ Create stub B&S assemblies for CI compilation
5. ✅ Run Auto Prefab Creator (creates prefabs from OBJ/GLB models)
6. ✅ Build AssetBundle for Android
7. ✅ Package mod ZIP with mod.json, icon.png, DLL, bundle, README

## Troubleshooting

If the build fails:
- Check the **Unity build log** artifact for details
- Verify the UNITY_LICENSE secret was pasted correctly (no extra whitespace)
- Ensure the XML starts with `<?xml version="1.0" encoding="UTF-8"?>`
- The license must have `com.unity.editor.headless` and `com.unity.editor.platforms.android` entitlements

## Local DLL Build (Required for Final Mod)

The CI build uses stub assemblies. For a production mod, you MUST build the real DLL locally:

```bash
# On your machine with B&S Nomad installed
export BLADE_SORCERY_PATH="/path/to/BladeAndSorcery_Data/Managed"
cd /home/wolfwolfwolf8586/JohnWickArsenal
./build.sh dll
```

Then copy the built DLL to the mod package before releasing.