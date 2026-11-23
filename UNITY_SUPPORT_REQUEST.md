## Unity Support Request

**Subject:** Unity Personal 2022.3.20f1 headless CI activation — UnityEntitlementLicense.xml not recognized, username/password returns HTTP 401

**Unity Version:** 2022.3.20f1 (LTS)  
**Platform:** Linux (GitHub Actions ubuntu-22.04 runners)  
**License Type:** Unity Personal  

---

### Verified Configuration

1. **Local entitlement active** — Unity Hub shows active Unity Personal license
2. **Local activation produces XML only** — `~/.config/unity3d/Unity/licenses/UnityEntitlementLicense.xml` exists; no `Unity_lic.ulf` file generated
3. **Licensing Client starts in Docker** — Container logs confirm:
   ```
   [Licensing::Module] Successfully launched the LicensingClient
   [Licensing::IpcConnector] Successfully connected to the License Client on channel: "LicenseClient-root"
   ```
4. **Cloud validation fails** — Editor exits with:
   ```
   [Licensing::Client] Error: Code 500... Errors: No ULF license found.,Token not found in cache
   [Licensing::Module] Error: License is not active (com.unity.editor.headless). HasEntitlements will fail.
   No valid Unity Editor license found.
   ```
5. **Username/password CLI returns HTTP 401** — Command:
   ```
   /opt/unity/Editor/Unity -batchmode -quit -nographics -projectPath /project \
     -username <email> -password <password> -serial ""
   ```
   Fails with:
   ```
   UnityConnectLoginRequest: Failed to login - please check your username or password
   No sufficient permissions while processing request "https://core.cloud.unity3d.com/api/login", HTTP error code 401
   ```

---

### Question

**Which officially supported method allows Unity Personal 2022.3.20f1 to authenticate in a headless CI environment (GitHub Actions) when:**

- The local activation produces only `UnityEntitlementLicense.xml` (no ULF file)
- The Licensing Client starts but reports "No ULF license found"
- Username/password authentication returns HTTP 401 (two-factor authentication is **enabled** on the account and must remain enabled)

**Please provide a documented, supported workflow that does not require:**
- Disabling two-factor authentication
- Generating a ULF file (not produced by current Unity Hub)
- Manual license activation (.alf/.ulf) — explicitly unsupported for Personal per [ManualActivationGuide](https://docs.unity3d.com/2022.3/Documentation/Manual/ManualActivationGuide.html)

---

### References

- [LicenseActivationMethods](https://docs.unity3d.com/2022.3/Documentation/Manual/LicenseActivationMethods.html) — states CLI works for headless Personal with internet
- [ManagingYourUnityLicense](https://docs.unity3d.com/2022.3/Documentation/Manual/ManagingYourUnityLicense.html) — states CLI procedures don't apply to Personal
- [Licensing Server Client Config](https://docs.unity.com/en-us/licensing-server/client-config) — covers only floating licenses (Enterprise/Industry)

---

**Request:** Confirm whether UnityEntitlementLicense.xml + Licensing Client is a supported headless CI configuration for Unity Personal, or document the supported alternative.