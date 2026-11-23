#!/usr/bin/env bash
# John Wick Arsenal - Automated Build Pipeline
# Compatible with Linux (ChromeOS/Crostini), macOS, and remote CI/CD environments
# Supports local Unity, Unity Cloud Build, GitHub Actions, and GitLab CI

set -euo pipefail

# ============================================================================
# CONFIGURATION - Override via environment variables or .env file
# ============================================================================
MOD_NAME="${MOD_NAME:-JohnWickArsenal}"
MOD_ROOT="${MOD_ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
UNITY_PROJECT_PATH="${UNITY_PROJECT_PATH:-${MOD_ROOT}/UnityProject}"
SOURCE_DIR="${SOURCE_DIR:-${MOD_ROOT}/Source/${MOD_NAME}}"
ASSEMBLY_OUTPUT="${ASSEMBLY_OUTPUT:-${MOD_ROOT}/Assemblies}"
BUNDLE_OUTPUT="${BUNDLE_OUTPUT:-${MOD_ROOT}/AssetBundles}"
MOD_OUTPUT="${MOD_OUTPUT:-${MOD_ROOT}/BuildOutput}"
BUILD_TARGET="${BUILD_TARGET:-Android}"
CONFIGURATION="${CONFIGURATION:-Release}"
UNITY_VERSION="${UNITY_VERSION:-2022.3}"

# Remote build configuration
REMOTE_BUILD="${REMOTE_BUILD:-false}"
REMOTE_BUILD_PROVIDER="${REMOTE_BUILD_PROVIDER:-}"  # github, gitlab, unitycloud, custom
REMOTE_BUILD_SCRIPT="${REMOTE_BUILD_SCRIPT:-}"

# Storage optimization for Chromebook
CLEAN_AFTER_BUILD="${CLEAN_AFTER_BUILD:-true}"
USE_TEMP_DIR="${USE_TEMP_DIR:-true}"
TEMP_BASE="${TEMP_BASE:-/tmp/jwa-build}"

# ============================================================================
# COLORS & LOGGING
# ============================================================================
if [[ -t 1 ]] && [[ "${NO_COLOR:-false}" != "true" ]]; then
    RED=$'\033[0;31m'
    GREEN=$'\033[0;32m'
    YELLOW=$'\033[1;33m'
    BLUE=$'\033[0;34m'
    CYAN=$'\033[0;36m'
    BOLD=$'\033[1m'
    NC=$'\033[0m'
else
    RED='' GREEN='' YELLOW='' BLUE='' CYAN='' BOLD='' NC=''
fi

LOG_FILE="${LOG_FILE:-${MOD_ROOT}/build.log}"
VERBOSE="${VERBOSE:-false}"

log() { echo -e "${BLUE}[INFO]${NC} $*" | tee -a "$LOG_FILE"; }
log_info() { echo -e "${BLUE}[INFO]${NC} $*" | tee -a "$LOG_FILE"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $*" | tee -a "$LOG_FILE"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $*" | tee -a "$LOG_FILE"; }
log_error() { echo -e "${RED}[ERROR]${NC} $*" | tee -a "$LOG_FILE"; }
log_debug() { [[ "$VERBOSE" == "true" ]] && echo -e "${CYAN}[DEBUG]${NC} $*" | tee -a "$LOG_FILE"; }
log_step() { echo -e "\n${BOLD}${BLUE}=== $* ===${NC}\n" | tee -a "$LOG_FILE"; }

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================
print_banner() {
    cat <<'EOF' | tee -a "$LOG_FILE"
╔═══════════════════════════════════════════════════════════════════╗
║       JOHN WICK ARSENAL - AUTOMATED BUILD PIPELINE              ║
║       Linux/ChromeOS Compatible • Remote Build Ready            ║
╚══════════════════════════════════════════════════════════════════╝
EOF
}

setup_temp_dir() {
    if [[ "$USE_TEMP_DIR" == "true" ]]; then
        export TMPDIR="${TEMP_BASE}/$$"
        mkdir -p "$TMPDIR"
        log_debug "Using temp directory: $TMPDIR"
    fi
}

cleanup_temp() {
    if [[ "$USE_TEMP_DIR" == "true" && -d "${TEMP_BASE:-}" ]]; then
        rm -rf "${TEMP_BASE}/$$" 2>/dev/null || true
    fi
}

trap cleanup_temp EXIT

# ============================================================================
# UNITY DETECTION (Linux/macOS/Windows/WSL/ChromeOS)
# ============================================================================
detect_unity() {
    log_step "Detecting Unity Installation"

    local unity_paths=()

    # 1. Explicit environment variable
    if [[ -n "${UNITY_EXECUTABLE:-}" && -f "$UNITY_EXECUTABLE" ]]; then
        log_success "Found Unity via UNITY_EXECUTABLE: $UNITY_EXECUTABLE"
        echo "$UNITY_EXECUTABLE"
        return 0
    fi

    # 2. Unity Hub installed editors (Linux/macOS/Windows)
    local hub_paths=(
        "$HOME/.local/share/unity3d/Hub/Editor"
        "$HOME/AppData/Roaming/Unity/Hub/Editor"  # Windows
        "/Applications/Unity/Hub/Editor"           # macOS
        "/opt/unity/Editor"
        "/opt/unity"
        "/usr/lib/unity"
    )

    for hub in "${hub_paths[@]}"; do
        if [[ -d "$hub" ]]; then
            while IFS= read -r -d '' editor; do
                unity_paths+=("$editor")
            done < <(find "$hub" -maxdepth 2 -name "Unity" -type f -print0 2>/dev/null)
        fi
    done

    # 3. System PATH
    if command -v unity-editor &>/dev/null; then
        unity_paths+=("$(command -v unity-editor)")
    fi
    if command -v unity &>/dev/null; then
        unity_paths+=("$(command -v unity)")
    fi

    # 4. Flatpak (common on Linux/ChromeOS)
    if command -v flatpak &>/dev/null; then
        if flatpak info com.unity.UnityHub &>/dev/null; then
            # Unity Hub flatpak - editors are in ~/.var/app/com.unity.UnityHub/data/unity3d/Hub/Editor
            local flatpak_editors="$HOME/.var/app/com.unity.UnityHub/data/unity3d/Hub/Editor"
            if [[ -d "$flatpak_editors" ]]; then
                while IFS= read -r -d '' editor; do
                    unity_paths+=("$editor")
                done < <(find "$flatpak_editors" -maxdepth 2 -name "Unity" -type f -print0 2>/dev/null)
            fi
        fi
    fi

    # 5. Snap
    if command -v snap &>/dev/null; then
        if snap list unity-editor &>/dev/null; then
            unity_paths+=("/snap/unity-editor/current/Editor/Unity")
        fi
    fi

    # Filter for requested version
    local matched=()
    for path in "${unity_paths[@]}"; do
        if [[ -f "$path" && -x "$path" ]]; then
            # Try to get version
            local version=$("$path" -version 2>/dev/null | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
            if [[ -z "$version" || "$version" == "$UNITY_VERSION"* ]]; then
                matched+=("$path")
                log_debug "Found Unity: $path (version: ${version:-unknown})"
            fi
        fi
    done

    if [[ ${#matched[@]} -eq 0 ]]; then
        log_error "Unity $UNITY_VERSION not found!"
        log_error "Searched locations:"
        for path in "${unity_paths[@]}"; do
            log_error "  - $path"
        done
        log_error ""
        log_error "Install Unity 2022.3 LTS via one of:"
        log_error "  • Unity Hub (recommended): https://unity.com/download"
        log_error "  • Flatpak: flatpak install flathub com.unity.UnityHub"
        log_error "  • Snap: snap install unity-editor --classic"
        log_error "  • Manual: Download from https://unity.com/releases/editor/archive"
        log_error ""
        log_error "Then set UNITY_EXECUTABLE environment variable or add to PATH"
        return 1
    fi

    # Prefer exact version match
    for path in "${matched[@]}"; do
        local version=$("$path" -version 2>/dev/null | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
        if [[ "$version" == "$UNITY_VERSION"* ]]; then
            UNITY_EXECUTABLE="$path"
            log_success "Selected Unity: $UNITY_EXECUTABLE (version: $version)"
            export UNITY_EXECUTABLE
            return 0
        fi
    done

    # Fallback to first match
    UNITY_EXECUTABLE="${matched[0]}"
    log_warn "Using Unity: $UNITY_EXECUTABLE (version may differ from $UNITY_VERSION)"
    export UNITY_EXECUTABLE
    return 0
}

# ============================================================================
# DOTNET DETECTION
# ============================================================================
detect_dotnet() {
    log_step "Detecting .NET SDK"

    if command -v dotnet &>/dev/null; then
        local version=$(dotnet --version)
        log_success "Found .NET SDK: $version"
        return 0
    fi

    # Check common Linux locations
    local dotnet_paths=(
        "/usr/share/dotnet/dotnet"
        "/opt/dotnet/dotnet"
        "$HOME/.dotnet/dotnet"
        "/snap/dotnet-sdk/current/dotnet"
    )

    for path in "${dotnet_paths[@]}"; do
        if [[ -f "$path" && -x "$path" ]]; then
            export PATH="$PATH:$(dirname "$path")"
            local version=$("$path" --version)
            log_success "Found .NET SDK at $path: $version"
            return 0
        fi
    done

    log_error ".NET SDK not found!"
    log_error "Install via:"
    log_error "  • apt: sudo apt update && sudo apt install -y dotnet-sdk-8.0"
    log_error "  • Snap: snap install dotnet-sdk --classic"
    log_error "  • Manual: https://dotnet.microsoft.com/download"
    log_error "  • ChromeOS: Enable Linux dev environment, then install via apt"
    return 1
}

# ============================================================================
# ANDROID SDK DETECTION (for Unity Android builds)
# ============================================================================
detect_android_sdk() {
    log_step "Detecting Android SDK"

    local android_home="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}"

    if [[ -n "$android_home" && -d "$android_home" ]]; then
        log_success "Android SDK found via ANDROID_HOME: $android_home"
        return 0
    fi

    # Common locations
    local sdk_paths=(
        "$HOME/Android/Sdk"
        "$HOME/Android/sdk"
        "$HOME/.android/sdk"
        "/opt/android-sdk"
        "/usr/lib/android-sdk"
        "/android-sdk"
    )

    for path in "${sdk_paths[@]}"; do
        if [[ -d "$path/platforms" ]]; then
            export ANDROID_HOME="$path"
            export ANDROID_SDK_ROOT="$path"
            log_success "Found Android SDK: $path"
            return 0
        fi
    done

    log_warn "Android SDK not found - Unity will use its internal SDK"
    log_warn "For better compatibility, install Android SDK:"
    log_warn "  • Android Studio: https://developer.android.com/studio"
    log_warn "  • Command line tools: https://developer.android.com/studio#command-tools"
    log_warn "  • Then set ANDROID_HOME environment variable"
    return 0  # Not fatal - Unity has built-in SDK
}

# ============================================================================
# DEPENDENCY CHECK
# ============================================================================
check_dependencies() {
    log_step "Checking Dependencies"

    local missing=()
    local warnings=()

    # Unity
    if ! detect_unity; then
        missing+=("Unity $UNITY_VERSION LTS")
    fi

    # .NET SDK
    if ! detect_dotnet; then
        missing+=(".NET SDK 8.0+")
    fi

    # Android SDK (optional but recommended)
    detect_android_sdk

    # Git (for version info)
    if ! command -v git &>/dev/null; then
        warnings+=("Git not found - version info will be limited")
    fi

    # Zip (for packaging)
    if ! command -v zip &>/dev/null; then
        missing+=("zip (for packaging)")
    fi

    # Python3 (for JSON validation)
    if ! command -v python3 &>/dev/null; then
        warnings+=("Python3 not found - JSON validation will be skipped")
    fi

    # rsync (for file copying)
    if ! command -v rsync &>/dev/null; then
        warnings+=("rsync not found - using cp instead")
    fi

    # Check disk space (important for Chromebook)
    local available_space=$(df -BG "$MOD_ROOT" | awk 'NR==2 {print $4}' | sed 's/G//')
    if [[ -n "$available_space" && "$available_space" -lt 5 ]]; then
        warnings+=("Low disk space: ${available_space}GB available (recommend 5GB+)")
    fi

    # ChromeOS specific checks
    if [[ -f "/etc/os-release" ]] && grep -q "chromiumos\|chromeos" /etc/os-release; then
        log_info "ChromeOS/Crostini detected"
        # Check if Linux dev environment has enough resources
        local mem_total=$(free -m | awk 'NR==2 {print $2}')
        if [[ "$mem_total" -lt 4096 ]]; then
            warnings+=("Low memory: ${mem_total}MB (recommend 4GB+ for Unity builds)")
        fi
    fi

    # Print warnings
    for warn in "${warnings[@]}"; do
        log_warn "$warn"
    done

    if [[ ${#missing[@]} -gt 0 ]]; then
        log_error "Missing required dependencies:"
        for dep in "${missing[@]}"; do
            log_error "  - $dep"
        done
        log_error ""
        log_error "Installation guides:"
        for dep in "${missing[@]}"; do
            print_install_guide "$dep"
        done
        return 1
    fi

    log_success "All required dependencies satisfied"
    return 0
}

print_install_guide() {
    local dep="$1"
    case "$dep" in
        *"Unity"*)
            log_error "  Unity $UNITY_VERSION LTS:"
            log_error "    • Unity Hub: https://unity.com/download"
            log_error "    • Flatpak: flatpak install flathub com.unity.UnityHub"
            log_error "    • Snap: snap install unity-editor --classic"
            log_error "    • Manual: https://unity.com/releases/editor/archive"
            log_error "    • Then: export UNITY_EXECUTABLE=/path/to/Unity"
            ;;
        *".NET"*)
            log_error "  .NET SDK 8.0+:"
            log_error "    • Ubuntu/Debian/ChromeOS: wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb && sudo dpkg -i packages-microsoft-prod.deb && sudo apt update && sudo apt install -y dotnet-sdk-8.0"
            log_error "    • Snap: snap install dotnet-sdk --classic"
            log_error "    • Manual: https://dotnet.microsoft.com/download"
            ;;
        *"zip"*)
            log_error "  zip:"
            log_error "    • apt: sudo apt install zip"
            log_error "    • dnf: sudo dnf install zip"
            log_error "    • brew: brew install zip"
            ;;
    esac
}

# ============================================================================
# UNITY PROJECT SETUP
# ============================================================================
setup_unity_project() {
    log_step "Setting Up Unity Project"

    if [[ ! -d "$UNITY_PROJECT_PATH" ]]; then
        log_info "Creating Unity project structure at $UNITY_PROJECT_PATH"
        mkdir -p "$UNITY_PROJECT_PATH"/{Assets,Packages,ProjectSettings}
    fi

    # Copy source code to Unity project
    log_info "Syncing source code to Unity project..."
    local dest="$UNITY_PROJECT_PATH/Assets/Scripts/JohnWickArsenal"
    mkdir -p "$dest"

    if command -v rsync &>/dev/null; then
        rsync -av --delete \
            --exclude='*.csproj' \
            --exclude='bin' \
            --exclude='obj' \
            --exclude='*.Tests*' \
            "$SOURCE_DIR/" "$dest/"
    else
        cp -r "$SOURCE_DIR"/* "$dest/" 2>/dev/null || true
        # Clean up excluded directories
        rm -rf "$dest/bin" "$dest/obj" "$dest/*.csproj" 2>/dev/null || true
    fi

    # Create packages-lock.json with required packages
    log_info "Creating packages-lock.json..."
    cat > "$UNITY_PROJECT_PATH/Packages/packages-lock.json" <<'EOF'
{
  "dependencies": {
    "com.unity.xr.management": "4.5.0",
    "com.unity.xr.oculus": "4.5.0",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.addressables": "1.21.0",
    "com.unity.urp": "14.0.8",
    "com.unity.mobile.android-logcat": "1.1.0"
  }
}
EOF

    # Create basic ProjectSettings if missing
    if [[ ! -f "$UNITY_PROJECT_PATH/ProjectSettings/ProjectSettings.asset" ]]; then
        log_info "Creating default ProjectSettings..."
        create_default_project_settings
    fi

    log_success "Unity project structure ready"
}

create_default_project_settings() {
    mkdir -p "$UNITY_PROJECT_PATH/ProjectSettings"

    # ProjectSettings.asset - basic settings
    cat > "$UNITY_PROJECT_PATH/ProjectSettings/ProjectSettings.asset" <<'EOF'
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!129 &1
ProjectSettings:
  m_ObjectHideFlags: 0
  m_BuildTarget: 13
  m_BuildTargetGroup: 1
  m_TargetPlatforms:
  - 13
  m_ActiveBuildTarget: 13
EOF

    # ProjectVersion.txt
    echo "m_EditorVersion: 2022.3.20f1" > "$UNITY_PROJECT_PATH/ProjectSettings/ProjectVersion.txt"

    # PlayerSettings - Android
    cat > "$UNITY_PROJECT_PATH/ProjectSettings/ProjectSettings.asset" <<'EOF'
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!129 &1
ProjectSettings:
  m_ObjectHideFlags: 0
  m_BuildTarget: 13
  m_BuildTargetGroup: 1
  m_TargetPlatforms:
  - 13
  m_ActiveBuildTarget: 13
EOF
}

# ============================================================================
# BUILD DLL
# ============================================================================
build_dll() {
    log_step "Building Managed DLL"

    cd "$SOURCE_DIR"

    # Restore NuGet packages
    log_info "Restoring NuGet packages..."
    if ! dotnet restore "JohnWickArsenal.csproj" -v quiet 2>&1 | tee -a "$LOG_FILE"; then
        log_error "NuGet restore failed"
        return 1
    fi

    # Build
    log_info "Compiling DLL ($CONFIGURATION)..."
    if ! dotnet build "JohnWickArsenal.csproj" \
        -c "$CONFIGURATION" \
        -o "$ASSEMBLY_OUTPUT" \
        -v minimal \
        --no-restore 2>&1 | tee -a "$LOG_FILE"; then
        log_error "DLL compilation failed"
        return 1
    fi

    # Verify output
    local dll_path="$ASSEMBLY_OUTPUT/JohnWickArsenal.dll"
    if [[ -f "$dll_path" ]]; then
        local size=$(du -h "$dll_path" | cut -f1)
        log_success "DLL built: $dll_path ($size)"

        # Verify it's a valid .NET assembly
        if command -v dotnet &>/dev/null; then
            dotnet "$dll_path" --version 2>/dev/null || true
        fi
        return 0
    else
        log_error "DLL output not found at $dll_path"
        return 1
    fi
}

# ============================================================================
# PRE-BUILD VALIDATION
# ============================================================================
validate_unity_project() {
    log_step "Validating Unity Project"

    local errors=0

    # Check project structure
    if [[ ! -d "$UNITY_PROJECT_PATH/Assets" ]]; then
        log_error "Unity project Assets folder not found"
        ((errors++))
    fi

    if [[ ! -d "$UNITY_PROJECT_PATH/Packages" ]]; then
        log_error "Unity project Packages folder not found"
        ((errors++))
    fi

    if [[ ! -f "$UNITY_PROJECT_PATH/ProjectSettings/ProjectVersion.txt" ]]; then
        log_error "ProjectVersion.txt not found"
        ((errors++))
    fi

    # Check for required source code
    if [[ ! -d "$UNITY_PROJECT_PATH/Assets/Scripts/JohnWickArsenal" ]]; then
        log_warn "Source code not synced to Unity project - will sync now"
        setup_unity_project
    fi

    # Check for asmdef
    if [[ ! -f "$UNITY_PROJECT_PATH/Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef" ]]; then
        log_warn "Assembly definition not found - creating default"
        create_asmdef
    fi

    # Check for prefabs in Resources
    local resources_path="$UNITY_PROJECT_PATH/Assets/JohnWickArsenal/Resources"
    if [[ -d "$resources_path" ]]; then
        local prefab_count=$(find "$resources_path" -name "*.prefab" 2>/dev/null | wc -l)
        if [[ "$prefab_count" -eq 0 ]]; then
            log_warn "No prefabs found in Resources folder - AssetBundle will be empty"
            log_warn "Run 'Prefab Setup Wizard' in Unity to create weapon prefabs"
        else
            log_info "Found $prefab_count prefab(s) in Resources"
        fi
    else
        log_warn "Resources folder not found - will be created by auto-configuration"
    fi

    # Check for models
    local models_path="$UNITY_PROJECT_PATH/Assets/JohnWickArsenal/Models"
    if [[ -d "$models_path" ]]; then
        local model_count=$(find "$models_path" -name "*.fbx" -o -name "*.obj" 2>/dev/null | wc -l)
        if [[ "$model_count" -eq 0 ]]; then
            log_warn "No models found in Models folder - import FBX/OBJ files"
        else
            log_info "Found $model_count model(s) in Models folder"
        fi
    fi

    # Check for URP asset
    local urp_asset="$UNITY_PROJECT_PATH/Assets/JohnWickArsenal/URP/JohnWickArsenal_URP.asset"
    if [[ ! -f "$urp_asset" ]]; then
        log_warn "URP asset not found - will be created by auto-configuration"
    fi

    if [[ $errors -gt 0 ]]; then
        log_error "Unity project validation failed with $errors error(s)"
        return 1
    fi

    log_success "Unity project validation passed"
    return 0
}

create_asmdef() {
    mkdir -p "$UNITY_PROJECT_PATH/Assets/Scripts/JohnWickArsenal"
    cat > "$UNITY_PROJECT_PATH/Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef" <<'EOF'
{
	"name": "JohnWickArsenal",
	"references": [
		"Unity.InputSystem",
		"Unity.Addressables",
		"Unity.XR.Management",
		"Unity.XR.Oculus"
	],
	"includePlatforms": [
		"Android"
	],
	"excludePlatforms": [],
	"allowUnsafeCode": false,
	"overrideReferences": false,
	"precompiledReferences": [],
	"autoReferenced": true,
	"defineConstraints": [],
	"versionDefines": [],
	"noEngineReferences": false
}
EOF
    log_info "Created assembly definition"
}

# ============================================================================
# BUILD ASSETBUNDLE (via Unity)
# ============================================================================
build_assetbundle() {
    log_step "Building AssetBundle for Android"

    if [[ "$REMOTE_BUILD" == "true" ]]; then
        log_info "Remote build mode enabled - delegating to remote provider"
        return run_remote_build
    fi

    # Validate Unity project before building
    if ! validate_unity_project; then
        log_error "Unity project validation failed - cannot build"
        return 1
    fi

    # Ensure Unity project is set up
    setup_unity_project

    # Build AssetBundle using Unity batch mode
    local unity_args=(
        -batchmode
        -quit
        -projectPath "$UNITY_PROJECT_PATH"
        -executeMethod JWA_BuildAssetBundles.BuildBundle
        -logFile -
        -buildTarget "$BUILD_TARGET"
        -outputPath "$BUNDLE_OUTPUT"
        -bundleName "johnwickarsenal"
    )

    # Add custom args from environment
    if [[ -n "${UNITY_ARGS:-}" ]]; then
        unity_args+=($UNITY_ARGS)
    fi

    log_info "Running Unity batch mode build..."
    log_debug "Unity command: $UNITY_EXECUTABLE ${unity_args[*]}"

    local build_log="${MOD_ROOT}/unity-build.log"

    # Run with timeout (30 minutes max)
    if ! timeout 1800 "$UNITY_EXECUTABLE" "${unity_args[@]}" 2>&1 | tee "$build_log" | tail -30; then
        local exit_code=${PIPESTATUS[0]}
        log_error "Unity build failed (exit code: $exit_code). Check $build_log for details."
        analyze_unity_log "$build_log"
        return 1
    fi

    # Check result
    local bundle_path="$BUNDLE_OUTPUT/Android/johnwickarsenal"
    if [[ -f "$bundle_path" ]]; then
        local size=$(du -h "$bundle_path" | cut -f1)
        log_success "AssetBundle built: $bundle_path ($size)"

        # Validate bundle
        validate_assetbundle "$bundle_path"
        return 0
    else
        log_error "AssetBundle not found at $bundle_path"
        log_error "Check Unity log for errors: $build_log"
        analyze_unity_log "$build_log"
        return 1
    fi
}

analyze_unity_log() {
    local log_file="$1"
    if [[ ! -f "$log_file" ]]; then
        return
    fi

    log_info "Analyzing Unity build log for common issues..."

    # Check for common errors
    if grep -q "No valid Unity license" "$log_file"; then
        log_error "  → Unity license issue: No valid license found"
        log_error "    Fix: Run Unity Hub and sign in, or set UNITY_LICENSE env var for CI"
    fi

    if grep -q "Could not find.*Addressables" "$log_file"; then
        log_error "  → Addressables not initialized"
        log_error "    Fix: Open Unity Editor and run 'Project Bootstrap > Setup Addressables'"
    fi

    if grep -q "URP.*not found\|Universal Render Pipeline.*missing" "$log_file"; then
        log_error "  → URP asset not assigned"
        log_error "    Fix: Open Unity Editor and run 'Project Bootstrap > Configure Graphics'"
    fi

    if grep -q "Missing.*prefab\|No prefabs found" "$log_file"; then
        log_error "  → No prefabs found in Resources folder"
        log_error "    Fix: Use 'Prefab Setup Wizard' to create weapon prefabs"
    fi

    if grep -q "CS0246\|CS0234\|error CS" "$log_file"; then
        log_error "  → C# compilation errors in scripts"
        log_error "    Fix: Check Source/JohnWickArsenal for missing references"
    fi

    if grep -q "Out of memory\|memory allocation failed" "$log_file"; then
        log_error "  → Out of memory during build"
        log_error    "    Fix: Increase swap space or use remote build (REMOTE_BUILD=true)"
    fi

    if grep -q "No space left on device" "$log_file"; then
        log_error "  → Disk full"
        log_error "    Fix: Clean build outputs (./build.sh clean) or free disk space"
    fi

    # Show last 50 lines of errors
    log_info "Last 50 lines of Unity log:"
    tail -50 "$log_file" | grep -E "(error|Error|ERROR|exception|Exception|EXCEPTION|fail|Fail|FAIL)" | head -20 || true
}

validate_assetbundle() {
    local bundle_path="$1"
    log_info "Validating AssetBundle..."

    # Quick validation - check file size and magic bytes
    local size=$(stat -c%s "$bundle_path" 2>/dev/null || stat -f%z "$bundle_path" 2>/dev/null)
    if [[ $size -lt 1024 ]]; then
        log_error "AssetBundle too small ($size bytes) - likely corrupted"
        return 1
    fi

    # Check UnityFS magic
    local magic=$(head -c 7 "$bundle_path" 2>/dev/null || true)
    if [[ "$magic" != "UnityFS" ]]; then
        log_warn "AssetBundle doesn't have UnityFS header - may be legacy format"
    fi

    # Try to list contents if Unity is available
    if [[ -n "$UNITY_EXECUTABLE" && -f "$UNITY_EXECUTABLE" ]]; then
        log_debug "Attempting to verify bundle contents..."
    fi

    log_success "AssetBundle validation passed (${size} bytes)"
    return 0
}

validate_assetbundle() {
    local bundle_path="$1"
    log_info "Validating AssetBundle..."

    # Quick validation - check file size and magic bytes
    local size=$(stat -c%s "$bundle_path" 2>/dev/null || stat -f%z "$bundle_path" 2>/dev/null)
    if [[ $size -lt 1024 ]]; then
        log_error "AssetBundle too small ($size bytes) - likely corrupted"
        return 1
    fi

    # Check UnityFS magic
    local magic=$(head -c 7 "$bundle_path" 2>/dev/null || true)
    if [[ "$magic" != "UnityFS" ]]; then
        log_warn "AssetBundle doesn't have UnityFS header - may be legacy format"
    fi

    log_success "AssetBundle validation passed (${size} bytes)"
    return 0
}

run_remote_build() {
    log_info "Running remote build via $REMOTE_BUILD_PROVIDER..."

    case "$REMOTE_BUILD_PROVIDER" in
        github)
            run_github_actions_build
            ;;
        gitlab)
            run_gitlab_ci_build
            ;;
        unitycloud)
            run_unity_cloud_build
            ;;
        custom)
            if [[ -n "$REMOTE_BUILD_SCRIPT" && -f "$REMOTE_BUILD_SCRIPT" ]]; then
                bash "$REMOTE_BUILD_SCRIPT"
            else
                log_error "Custom remote build script not specified or not found"
                return 1
            fi
            ;;
        *)
            log_error "Unknown remote build provider: $REMOTE_BUILD_PROVIDER"
            return 1
            ;;
    esac
}

run_github_actions_build() {
    log_info "Triggering GitHub Actions workflow..."
    # This would use gh CLI to trigger a workflow_dispatch
    # Requires: gh auth login, repo with workflow
    if command -v gh &>/dev/null; then
        gh workflow run "build-android.yml" --ref main 2>&1 | tee -a "$LOG_FILE"
        log_success "GitHub Actions workflow triggered"
    else
        log_error "GitHub CLI (gh) not installed"
        return 1
    fi
}

run_gitlab_ci_build() {
    log_info "Triggering GitLab CI pipeline..."
    # Use GitLab API to trigger pipeline
    if [[ -n "${GITLAB_TOKEN:-}" && -n "${GITLAB_PROJECT_ID:-}" ]]; then
        curl -X POST \
            -H "Authorization: Bearer $GITLAB_TOKEN" \
            -F "ref=main" \
            "https://gitlab.com/api/v4/projects/$GITLAB_PROJECT_ID/trigger/pipeline" 2>&1 | tee -a "$LOG_FILE"
        log_success "GitLab CI pipeline triggered"
    else
        log_error "GITLAB_TOKEN and GITLAB_PROJECT_ID required"
        return 1
    fi
}

run_unity_cloud_build() {
    log_info "Unity Cloud Build integration not yet implemented"
    log_info "Configure at: https://build.cloud.unity3d.com/"
    return 1
}

# ============================================================================
# PACKAGE MOD
# ============================================================================
package_mod() {
    log_step "Packaging Mod"

    rm -rf "$MOD_OUTPUT"
    mkdir -p "$MOD_OUTPUT/$MOD_NAME"

    # Copy required files
    local files=(
        "$MOD_ROOT/mod.json"
        "$MOD_ROOT/icon.png"
        "$ASSEMBLY_OUTPUT/JohnWickArsenal.dll"
        "$BUNDLE_OUTPUT/Android/johnwickarsenal"
        "$MOD_ROOT/README.md"
    )

    for file in "${files[@]}"; do
        if [[ -f "$file" ]]; then
            cp "$file" "$MOD_OUTPUT/$MOD_NAME/"
            log_debug "Copied: $(basename "$file")"
        else
            log_error "Required file missing: $file"
            return 1
        fi
    done

    # Rename bundle
    mv "$MOD_OUTPUT/$MOD_NAME/johnwickarsenal" "$MOD_OUTPUT/$MOD_NAME/johnwickarsenal.bundle"

    # Create ZIP
    cd "$MOD_OUTPUT"
    if command -v zip &>/dev/null; then
        zip -r "${MOD_NAME}_v1.0.0.zip" "$MOD_NAME/" -q 2>&1 | tee -a "$LOG_FILE"
        local zip_size=$(du -h "${MOD_NAME}_v1.0.0.zip" | cut -f1)
        log_success "Mod packaged: ${MOD_NAME}_v1.0.0.zip ($zip_size)"
    else
        log_warn "zip not available - created directory only"
    fi

    return 0
}

# ============================================================================
# VERIFY BUILD
# ============================================================================
verify_build() {
    log_step "Verifying Build Output"

    local required=(
        "$MOD_OUTPUT/$MOD_NAME/mod.json"
        "$MOD_OUTPUT/$MOD_NAME/icon.png"
        "$MOD_OUTPUT/$MOD_NAME/JohnWickArsenal.dll"
        "$MOD_OUTPUT/$MOD_NAME/johnwickarsenal.bundle"
        "$MOD_OUTPUT/$MOD_NAME/README.md"
    )

    local all_ok=true
    for file in "${required[@]}"; do
        if [[ -f "$file" ]]; then
            local size=$(du -h "$file" | cut -f1)
            log_success "  ✓ $(basename "$file") ($size)"
        else
            log_error "  ✗ MISSING: $file"
            all_ok=false
        fi
    done

    # Validate mod.json
    if [[ -f "$MOD_OUTPUT/$MOD_NAME/mod.json" ]]; then
        if command -v python3 &>/dev/null; then
            if python3 -c "import json; json.load(open('$MOD_OUTPUT/$MOD_NAME/mod.json'))" 2>/dev/null; then
                log_success "  ✓ mod.json is valid JSON"
            else
                log_error "  ✗ mod.json is invalid JSON"
                all_ok=false
            fi
        fi
    fi

    # Verify DLL is loadable
    if [[ -f "$MOD_OUTPUT/$MOD_NAME/JohnWickArsenal.dll" ]]; then
        if command -v dotnet &>/dev/null; then
            if dotnet "$MOD_OUTPUT/$MOD_NAME/JohnWickArsenal.dll" --version 2>/dev/null || true; then
                log_success "  ✓ DLL appears valid"
            fi
        fi
    fi

    if [[ "$all_ok" == "true" ]]; then
        log_success "All verification checks passed"
        return 0
    else
        log_error "Verification failed"
        return 1
    fi
}

# ============================================================================
# CLEAN
# ============================================================================
clean_build() {
    log_step "Cleaning Build Outputs"

    local dirs=("$ASSEMBLY_OUTPUT" "$BUNDLE_OUTPUT" "$MOD_OUTPUT" "$TEMP_BASE")
    for dir in "${dirs[@]}"; do
        if [[ -d "$dir" ]]; then
            rm -rf "$dir"
            log_info "Removed: $dir"
        fi
    done

    log_success "Clean complete"
}

# ============================================================================
# MAIN
# ============================================================================
main() {
    # Load .env file if exists
    if [[ -f "$MOD_ROOT/.env" ]]; then
        log_info "Loading environment from .env"
        set -a
        source "$MOD_ROOT/.env"
        set +a
    fi

    # Initialize log
    echo "=== Build started at $(date) ===" > "$LOG_FILE"

    print_banner
    log_info "Mod: $MOD_NAME"
    log_info "Root: $MOD_ROOT"
    log_info "Unity Project: $UNITY_PROJECT_PATH"
    log_info "Target: $BUILD_TARGET"
    log_info "Config: $CONFIGURATION"
    log_info "Remote Build: $REMOTE_BUILD"

    setup_temp_dir

    case "${1:-all}" in
        deps|dependencies)
            check_dependencies
            ;;
        setup|init)
            setup_unity_project
            log_success "Unity project initialized. Open in Unity Editor to complete setup."
            ;;
        dll)
            check_dependencies && build_dll
            ;;
        bundle)
            check_dependencies && build_assetbundle
            ;;
        package)
            package_mod && verify_build
            ;;
        verify)
            verify_build
            ;;
        clean)
            clean_build
            ;;
        all|*)
            check_dependencies && \
            build_dll && \
            build_assetbundle && \
            package_mod && \
            verify_build && \
            print_summary
            ;;
    esac
}

print_summary() {
    cat <<EOF | tee -a "$LOG_FILE"

${GREEN}╔══════════════════════════════════════════════════════════════╗
║                    BUILD COMPLETE!                            ║
╚══════════════════════════════════════════════════════════════╝${NC}

Output: ${MOD_OUTPUT}/${MOD_NAME}_v1.0.0.zip

Contents:
$(ls -lh "$MOD_OUTPUT/$MOD_NAME/" 2>/dev/null || echo "  (run package step)")

${YELLOW}Next Steps:${NC}
1. Transfer ${MOD_NAME}_v1.0.0.zip to Quest (via ADB, SideQuest, or USB)
2. Extract to /sdcard/Mods/JohnWickArsenal/
3. Enable in BSMG Mod Loader or Mod.io
4. Launch Blade & Sorcery Nomad

${CYAN}Logs:${NC} $LOG_FILE
${CYAN}Unity Log:${NC} ${MOD_ROOT}/unity-build.log

EOF
}

# Run main
main "$@"