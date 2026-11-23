#!/usr/bin/env bash

# John Wick Arsenal - Structure Verification Script
# Run this to verify all files are in place before building

MOD_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Colors
if [[ -t 1 ]]; then
    GREEN=$'\033[0;32m'
    RED=$'\033[0;31m'
    YELLOW=$'\033[1;33m'
    BLUE=$'\033[0;34m'
    BOLD=$'\033[1m'
    NC=$'\033[0m'
else
    GREEN='' RED='' YELLOW='' BLUE='' BOLD='' NC=''
fi

check_file() {
    if [[ -f "$1" ]]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1 (MISSING)"
        return 1
    fi
}

check_dir() {
    if [[ -d "$1" ]]; then
        echo -e "${GREEN}✓${NC} $1/"
        return 0
    else
        echo -e "${YELLOW}⚠${NC} $1/ (NOT FOUND - will be created)"
        return 1
    fi
}

MISSING=0
WARNINGS=0

echo -e "${BOLD}${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BOLD}${BLUE}║         JOHN WICK ARSENAL - STRUCTURE VERIFICATION          ║${NC}"
echo -e "${BOLD}${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

echo "=== ROOT FILES ==="
check_file "$MOD_ROOT/mod.json" || ((MISSING++))
check_file "$MOD_ROOT/README.md" || ((MISSING++))
check_file "$MOD_ROOT/icon.png" || ((MISSING++))
check_file "$MOD_ROOT/build.sh" || ((MISSING++))
check_file "$MOD_ROOT/setup.sh" || ((WARNINGS++))
check_file "$MOD_ROOT/.env.example" || ((WARNINGS++))
check_file "$MOD_ROOT/.gitignore" || ((WARNINGS++))
check_file "$MOD_ROOT/.gitlab-ci.yml" || ((WARNINGS++))

echo ""
echo "=== SOURCE CODE (C#) ==="
check_file "$MOD_ROOT/Source/JohnWickArsenal/JohnWickArsenal.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/JohnWickArsenal.csproj" || ((MISSING++))

echo ""
echo "=== WEAPON BEHAVIORS ==="
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/JohnWickPistolBehavior.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/JohnWickShotgunBehavior.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/JohnWickRevolverBehavior.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/JohnWickRifleBehavior.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/ThrowingKnifeBehavior.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Behaviors/PencilBehavior.cs" || ((MISSING++))

echo ""
echo "=== UTILS & DATA ==="
check_file "$MOD_ROOT/Source/JohnWickArsenal/Utils/ModBehaviorRegistry.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Data/ConfigLoader.cs" || ((MISSING++))
check_file "$MOD_ROOT/Source/JohnWickArsenal/Tests/WeaponDataTests.cs" || ((WARNINGS++))

echo ""
echo "=== EDITOR TOOLS (Unity) ==="
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_ProjectBootstrapper.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_PrefabSetupWizard.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_BuildAssetBundles.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_BuildValidator.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_DependencyChecker.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_AutoConfiguration.cs" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/Assets/Editor/JWA_AssetImporter.cs" || ((MISSING++))

echo ""
echo "=== UNITY PROJECT SETTINGS ==="
check_file "$MOD_ROOT/UnityProject/ProjectSettings/ProjectVersion.txt" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/ProjectSettings/ProjectSettings.asset" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/ProjectSettings/PlayerSettings.asset" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/ProjectSettings/GraphicsSettings.asset" || ((MISSING++))
check_file "$MOD_ROOT/UnityProject/ProjectSettings/QualitySettings.asset" || ((WARNINGS++))
check_file "$MOD_ROOT/UnityProject/ProjectSettings/TagManager.asset" || ((WARNINGS++))
check_file "$MOD_ROOT/UnityProject/Packages/packages-lock.json" || ((MISSING++))

echo ""
echo "=== UNITY ASSEMBLY DEFINITION ==="
check_file "$MOD_ROOT/UnityProject/Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef" || ((MISSING++))

echo ""
echo "=== CI/CD PIPELINES ==="
check_file "$MOD_ROOT/.github/workflows/build-android.yml" || ((WARNINGS++))

echo ""
echo "=== DIRECTORIES (auto-created if missing) ==="
check_dir "$MOD_ROOT/AssetBundles"
check_dir "$MOD_ROOT/Assemblies"
check_dir "$MOD_ROOT/BuildOutput"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Models"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Textures"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Audio"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Animations"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Resources"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/Resources/Weapons"
check_dir "$MOD_ROOT/UnityProject/Assets/JohnWickArsenal/URP"

echo ""
echo -e "${BOLD}${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
if [[ $MISSING -eq 0 ]]; then
    echo -e "${BOLD}${BLUE}║  ${GREEN}ALL REQUIRED CHECKS PASSED${NC} - Ready for Unity build!      ║${NC}"
else
    echo -e "${BOLD}${BLUE}║  ${RED}$MISSING REQUIRED FILES MISSING${NC} - Fix before building        ║${NC}"
fi
echo -e "${BOLD}${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"

if [[ $WARNINGS -gt 0 ]]; then
    echo -e "${YELLOW}⚠ $WARNINGS optional files/directories not found (will be created on build)${NC}"
fi

# Show file counts
echo ""
echo "=== STATISTICS ==="
CS_FILES=$(find "$MOD_ROOT/Source" -name "*.cs" 2>/dev/null | wc -l)
EDITOR_FILES=$(find "$MOD_ROOT/UnityProject/Assets/Editor" -name "*.cs" 2>/dev/null | wc -l)
MD_FILES=$(find "$MOD_ROOT" -name "*.md" 2>/dev/null | wc -l)
TOTAL_LINES=$(find "$MOD_ROOT/Source" -name "*.cs" -exec wc -l {} + 2>/dev/null | tail -1 | awk '{print $1}')

echo "C# Source Scripts:     $CS_FILES files"
echo "Unity Editor Scripts:  $EDITOR_FILES files"
echo "Documentation:         $MD_FILES files"
echo "Total Source Lines:    ~${TOTAL_LINES:-0} lines"

# Check Unity project
echo ""
echo "=== UNITY PROJECT STATUS ==="
if [[ -f "$MOD_ROOT/UnityProject/ProjectSettings/ProjectVersion.txt" ]]; then
    UNITY_VER=$(cat "$MOD_ROOT/UnityProject/ProjectSettings/ProjectVersion.txt" | grep -oE '[0-9]+\.[0-9]+\.[0-9]+f[0-9]+' | head -1)
    echo "Unity Version: $UNITY_VER"
fi

if [[ -f "$MOD_ROOT/UnityProject/Packages/packages-lock.json" ]]; then
    PKG_COUNT=$(grep -c '"version"' "$MOD_ROOT/UnityProject/Packages/packages-lock.json" || echo 0)
    echo "Packages configured: $PKG_COUNT"
fi

exit $MISSING