#!/bin/bash

# Setup script for GitHub branch protection
# Requires GitHub CLI (gh) to be installed and authenticated

set -e

REPO="purlieu-studios/verbose-waffle"
REQUIRED_CHECK="Ensure Rules Haven't Been Weakened"

echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  Branch Protection Setup for Code Quality Rules          ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""

# Check if gh is installed
if ! command -v gh &> /dev/null; then
    echo "❌ Error: GitHub CLI (gh) is not installed"
    echo ""
    echo "Install it from: https://cli.github.com/"
    echo "Or use the manual setup instructions in .github/BRANCH_PROTECTION.md"
    exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
    echo "❌ Error: Not authenticated with GitHub CLI"
    echo ""
    echo "Run: gh auth login"
    exit 1
fi

echo "✅ GitHub CLI is installed and authenticated"
echo ""

# Function to setup branch protection
setup_branch_protection() {
    local branch=$1
    echo "Setting up branch protection for: $branch"

    # Create branch protection rule
    gh api \
        --method PUT \
        -H "Accept: application/vnd.github+json" \
        "/repos/$REPO/branches/$branch/protection" \
        -f required_status_checks[strict]=true \
        -f required_status_checks[contexts][]="$REQUIRED_CHECK" \
        -f required_pull_request_reviews[dismiss_stale_reviews]=true \
        -f required_pull_request_reviews[require_code_owner_reviews]=true \
        -f required_pull_request_reviews[required_approving_review_count]=1 \
        -f required_pull_request_reviews[require_last_push_approval]=false \
        -f enforce_admins=false \
        -f required_conversation_resolution=true \
        -f allow_force_pushes=false \
        -f allow_deletions=false \
        -f block_creations=false \
        -f required_linear_history=false \
        -f restrictions=null \
        > /dev/null 2>&1

    if [ $? -eq 0 ]; then
        echo "  ✅ Branch protection configured for $branch"
    else
        echo "  ⚠️  Warning: Could not configure branch protection for $branch"
        echo "     You may need admin permissions or the branch may not exist yet"
        echo "     See .github/BRANCH_PROTECTION.md for manual setup"
    fi
}

# Setup protection for main and dev branches
echo "Configuring branch protection rules..."
echo ""

setup_branch_protection "main"
setup_branch_protection "dev"

echo ""
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  Setup Complete!                                          ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""
echo "Branch protection is now active with:"
echo "  ✅ Pull requests required for main and dev"
echo "  ✅ CI status check required: \"$REQUIRED_CHECK\""
echo "  ✅ Code owner review required (CODEOWNERS)"
echo "  ✅ Conversation resolution required"
echo ""
echo "Next steps:"
echo "  1. Verify protection at: https://github.com/$REPO/settings/branches"
echo "  2. Add maintainers to GitHub team or update CODEOWNERS"
echo "  3. Test by creating a PR that modifies CodeAnalysis.ruleset"
echo ""
echo "For more information, see .github/BRANCH_PROTECTION.md"
