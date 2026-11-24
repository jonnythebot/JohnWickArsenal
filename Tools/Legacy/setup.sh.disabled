#!/usr/bin/env bash
# John Wick Arsenal - First-time Setup Script
# Run this once after cloning the repository to initialize the development environment

set -euo pipefail

# Colors
if [[ -t 1 ]]; then
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

log_info() { echo -e "${BLUE}[INFO]${NC} $*"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $*"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }
log_error() { echo -e "${RED}[ERROR]${NC} $*"; }
log_step() { echo -e "\n${BOLD}${BLUE}=== $* ===${NC}\n"; }

MOD_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

print_banner() {
    cat <<'EOF'
╔═══════════════════════════════════════════════════════════════════╗
║       JOHN WICK ARSENAL - DEVELOPMENT SETUP                     ║
║       Initializes project for Blade & Sorcery Nomad mod dev     ║
╚══════════════════════════════════════════════════════════════════╝
EOF
}

check_os() {
    log_step "Detecting Operating System"

    if [[ -f "/etc/os-release" ]]; then
        source /etc/os-release
        log_info "OS: $PRETTY_NAME"

        if [[ "$ID" == "debian" || "$ID" == "ubuntu" || "$ID_LIKE" == *"debian"* ]]; then
            PKG_MANAGER="apt"
            INSTALL_CMD="sudo apt update && sudo apt install -y"
        elif [[ "$ID" == "fedora" || "$ID" == "rhel" || "$ID_LIKE" == *"fedora"* ]]; then
            PKG_MANAGER="dnf"
            INSTALL_CMD="sudo dnf install -y"
        elif [[ "$ID" == "arch" || "$ID_LIKE" == *"arch"* ]]; then
            PKG_MANAGER="pacman"
            INSTALL_CMD="sudo pacman -S --noconfirm"
        else
            PKG_MANAGER="unknown"
        fi
    elif [[ "$(uname)" == "Darwin" ]]; then
        log_info "OS: macOS"
        PKG_MANAGER="brew"
        INSTALL_CMD="brew install"
    else
        log_info "OS: $(uname -s)"
        PKG_MANAGER="unknown"
    fi

    # Check for ChromeOS
    if grep -q "chromiumos\|chromeos" /etc/os-release 2>/dev/null; then
        log_info "ChromeOS/Crostini detected - using Linux dev environment"
        IS_CHROMEOS=true
    fi
}

install_system_deps() {
    log_step "Installing System Dependencies"

    local deps=()

    case "$PKG_MANAGER" in
        apt)
            deps=(git zip unzip curl wget python3 rsync openjdk-11-jdk-headless)
            ;;
        dnf)
            deps=(git zip unzip curl wget python3 rsync java-11-openjdk-headless)
            ;;
        pacman)
            deps=(git zip unzip curl wget python rsync jdk11-openjdk)
            ;;
        brew)
            deps=(git zip unzip curl wget python3 rsync openjdk@11)
            ;;
        *)
            log_warn "Unknown package manager - please install manually: git, zip, curl, wget, python3, rsync, java"
            return
            ;;
    esac

    log_info "Installing: ${deps[*]}"
    if eval "$INSTALL_CMD ${deps[*]}"; then
        log_success "System dependencies installed"
    else
        log_error "Failed to install some dependencies"
        return 1
    fi
}

install_dotnet() {
    log_step "Installing .NET SDK 8.0"

    if command -v dotnet &>/dev/null; then
        local version=$(dotnet --version)
        if [[ "$version" == 8.* ]]; then
            log_success ".NET SDK already installed: $version"
            return 0
        fi
    fi

    case "$PKG_MANAGER" in
        apt)
            log_info "Installing .NET SDK via Microsoft repository..."
            wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
            sudo dpkg -i /tmp/packages-microsoft-prod.deb
            sudo apt update
            sudo apt install -y dotnet-sdk-8.0
            ;;
        dnf)
            sudo dnf install -y dotnet-sdk-8.0
            ;;
        pacman)
            sudo pacman -S --noconfirm dotnet-sdk
            ;;
        brew)
            brew install dotnet-sdk
            ;;
        *)
            log_warn "Please install .NET SDK 8.0 manually: https://dotnet.microsoft.com/download"
            return 1
            ;;
    esac

    if command -v dotnet &>/dev/null; then
        log_success ".NET SDK installed: $(dotnet --version)"
    else
        log_error ".NET SDK installation failed"
        return 1
    fi
}

install_unity() {
    log_step "Installing Unity 2022.3 LTS"

    if command -v unity-editor &>/dev/null || command -v unity &>/dev/null; then
        log_success "Unity already in PATH"
        return 0
    fi

    # Check Unity Hub flatpak
    if command -v flatpak &>/dev/null; then
        if flatpak info com.unity.UnityHub &>/dev/null; then
            log_success "Unity Hub already installed via Flatpak"
            log_info "Install Unity 2022.3 LTS via Unity Hub GUI"
            return 0
        fi
    fi

    # Check Snap
    if command -v snap &>/dev/null; then
        if snap list unity-editor &>/dev/null; then
            log_success "Unity Editor already installed via Snap"
            return 0
        fi
    fi

    log_info "Unity Editor not found. Choose installation method:"
    echo "  1) Unity Hub (recommended) - https://unity.com/download"
    echo "  2) Flatpak: flatpak install flathub com.unity.UnityHub"
    echo "  3) Snap: snap install unity-editor --classic"
    echo "  4) Manual download from https://unity.com/releases/editor/archive"

    read -p "Install Unity Hub via Flatpak? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        if command -v flatpak &>/dev/null; then
            flatpak install -y flathub com.unity.UnityHub
            log_success "Unity Hub installed. Run it and install Unity 2022.3 LTS."
        else
            log_error "Flatpak not available"
        fi
    fi
}

install_android_sdk() {
    log_step "Installing Android SDK (Command Line Tools)"

    if [[ -n "${ANDROID_HOME:-}" && -d "$ANDROID_HOME/platforms" ]]; then
        log_success "Android SDK already installed at $ANDROID_HOME"
        return 0
    fi

    local sdk_dir="$HOME/Android/Sdk"
    if [[ -d "$sdk_dir/platforms" ]]; then
        log_success "Android SDK found at $sdk_dir"
        export ANDROID_HOME="$sdk_dir"
        export ANDROID_SDK_ROOT="$sdk_dir"
        return 0
    fi

    read -p "Install Android SDK command line tools? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_warn "Skipping Android SDK installation (Unity has built-in SDK)"
        return 0
    fi

    mkdir -p "$sdk_dir/cmdline-tools"
    cd "$sdk_dir"

    log_info "Downloading command line tools..."
    wget -q "https://dl.google.com/android/repository/commandlinetools-linux-10406996_latest.zip" -O /tmp/cmdtools.zip
    unzip -q /tmp/cmdtools.zip -d cmdline-tools/latest

    export ANDROID_HOME="$sdk_dir"
    export ANDROID_SDK_ROOT="$sdk_dir"
    export PATH="$PATH:$sdk_dir/platform-tools:$sdk_dir/cmdline-tools/latest/bin"

    log_info "Accepting licenses and installing packages..."
    yes | cmdline-tools/latest/bin/sdkmanager --licenses
    cmdline-tools/latest/bin/sdkmanager \
        "platform-tools" \
        "platforms;android-33" \
        "build-tools;33.0.0" \
        "ndk;25.1.8937393"

    # Add to shell profile
    local shell_rc=""
    if [[ -f "$HOME/.bashrc" ]]; then shell_rc="$HOME/.bashrc"; fi
    if [[ -f "$HOME/.zshrc" ]]; then shell_rc="$HOME/.zshrc"; fi

    if [[ -n "$shell_rc" ]]; then
        if ! grep -q "ANDROID_HOME" "$shell_rc"; then
            cat >> "$shell_rc" <<'EOF'

# Android SDK
export ANDROID_HOME=$HOME/Android/Sdk
export ANDROID_SDK_ROOT=$HOME/Android/Sdk
export PATH=$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin
EOF
            log_info "Added Android SDK to $shell_rc"
        fi
    fi

    log_success "Android SDK installed at $sdk_dir"
}

setup_unity_project() {
    log_step "Setting Up Unity Project"

    cd "$MOD_ROOT"

    # Run build.sh setup
    if ./build.sh setup; then
        log_success "Unity project structure created"
    else
        log_error "Failed to set up Unity project"
        return 1
    fi
}

build_dll() {
    log_step "Building Managed DLL"

    if ./build.sh dll; then
        log_success "DLL built successfully"
    else
        log_error "DLL build failed"
        return 1
    fi
}

create_env_file() {
    log_step "Creating Environment Configuration"

    if [[ ! -f "$MOD_ROOT/.env" ]]; then
        cp "$MOD_ROOT/.env.example" "$MOD_ROOT/.env"
        log_success "Created .env from .env.example"
        log_info "Edit .env to customize build settings"
    else
        log_info ".env already exists"
    fi
}

verify_setup() {
    log_step "Verifying Setup"

    cd "$MOD_ROOT"
    if ./build.sh deps; then
        log_success "All dependencies satisfied!"
    else
        log_warn "Some dependencies missing - see above"
    fi
}

main() {
    print_banner

    log_info "Project root: $MOD_ROOT"

    check_os

    # Run setup steps
    install_system_deps
    install_dotnet
    install_unity
    install_android_sdk
    setup_unity_project
    build_dll
    create_env_file
    verify_setup

    cat <<EOF

${GREEN}╔══════════════════════════════════════════════════════════════╗
║                    SETUP COMPLETE!                            ║
╚══════════════════════════════════════════════════════════════╝${NC}

Next steps:

1. ${CYAN}Open Unity Editor${NC} and load the project:
   ${MOD_ROOT}/UnityProject

2. ${CYAN}Run Project Bootstrap${NC} in Unity:
   Menu: John Wick Arsenal > Project Bootstrap
   Click "Run Full Bootstrap"

3. ${CYAN}Import weapon models${NC} to:
   Assets/JohnWickArsenal/Models/

4. ${CYAN}Create prefabs${NC} using:
   Menu: John Wick Arsenal > Prefab Setup Wizard

5. ${CYAN}Build AssetBundle${NC}:
   Menu: John Wick Arsenal > Build AssetBundle
   OR command line: ./build.sh bundle

6. ${CYAN}Full build pipeline${NC}:
   ./build.sh all

${YELLOW}For Chromebook/limited storage:${NC}
- Use remote build: REMOTE_BUILD=true REMOTE_BUILD_PROVIDER=github ./build.sh all
- Or push to GitHub/GitLab to trigger CI/CD

${CYAN}Documentation:${NC}
- README.md - Project overview
- BUILD_SYSTEM.md - Build system details
- Unity Editor: John Wick Arsenal menu

EOF
}

# Run main
main "$@"