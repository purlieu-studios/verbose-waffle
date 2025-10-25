# Branch Protection Setup

This document explains how to configure GitHub branch protection to prevent weakening code quality rules.

## Protection Strategy

**Balanced Approach:**
- ✅ Require pull requests for all changes to `main` and `dev`
- ✅ Require status checks to pass (CI validation)
- ✅ Allow maintainers to approve their own PRs (flexibility for solo/small teams)
- ✅ Automatically run validation on both PRs and direct pushes

This gives you flexibility while catching any rule-weakening attempts through automated CI.

## Setup Instructions

### Option 1: Automated Setup (Recommended)

If you have GitHub CLI installed:

```bash
# Run the setup script
./.github/scripts/setup-branch-protection.sh
```

### Option 2: Manual Setup via GitHub Web UI

1. **Navigate to Repository Settings**
   - Go to `https://github.com/purlieu-studios/verbose-waffle/settings`
   - Click "Branches" in the left sidebar

2. **Add Branch Protection Rule for `main`**
   - Click "Add rule"
   - Branch name pattern: `main`
   - Enable the following settings:
     - ✅ **Require a pull request before merging**
       - ✅ Require approvals: `1`
       - ✅ Allow specified actors to bypass required pull requests
         - Add: `@purlieu-studios/maintainers` or your username
     - ✅ **Require status checks to pass before merging**
       - ✅ Require branches to be up to date before merging
       - Add required status check: `Ensure Rules Haven't Been Weakened`
     - ✅ **Require conversation resolution before merging**
     - ✅ **Do not allow bypassing the above settings**
   - Click "Create" or "Save changes"

3. **Add Branch Protection Rule for `dev`**
   - Repeat the same steps for branch pattern: `dev`

4. **Configure CODEOWNERS Enforcement** (Optional but Recommended)
   - In the same branch protection rule:
     - ✅ **Require review from Code Owners**
   - This ensures changes to protected files require maintainer approval

## What This Protects Against

### Scenario 1: You Try to Weaken Rules Locally
1. Pre-commit hook blocks the commit ❌
2. You use `--no-verify` to bypass
3. You create a PR (required by branch protection)
4. CI runs and fails if rules weakened ❌
5. PR cannot be merged until CI passes

### Scenario 2: AI Assistant Tries to Weaken Rules
1. If working in PR flow: Same as Scenario 1
2. If somehow bypassing: CI runs on push and fails ❌

### Scenario 3: Emergency Rule Change Needed
1. Create PR with rule change
2. Document rationale in `CODE_QUALITY_RULES.md`
3. CI will warn but you can still approve your own PR
4. Merge with documented justification
5. Change is tracked in git history

## Verifying Protection is Active

### Check Pre-commit Hook
```bash
# Should be: .githooks
git config core.hooksPath
```

### Check Branch Protection
```bash
# Using GitHub CLI
gh api repos/purlieu-studios/verbose-waffle/branches/main/protection | jq '.required_pull_request_reviews, .required_status_checks'
```

### Test the Protection
```bash
# This should be blocked by pre-commit hook
echo "# test" >> CodeAnalysis.ruleset
git add CodeAnalysis.ruleset
git commit -m "test"
# ❌ BLOCKED

# This should succeed (hook allows non-protected files)
echo "# test" >> README.md
git add README.md
git commit -m "test"
# ✅ ALLOWED
```

## Maintenance

### Adding New Protected Files

If you add new configuration files that should be protected:

1. Update `.githooks/pre-commit` to include the new file
2. Update `.github/workflows/validate-rules.yml` paths
3. Update `.github/CODEOWNERS` to require approval
4. Document in this file

### Rotating Maintainers

To change who can approve rule changes:

1. Update the GitHub team `@purlieu-studios/maintainers`
2. Or update `.github/CODEOWNERS` with specific usernames

## FAQ

**Q: Can I push directly to `dev` or `main`?**
A: No, branch protection requires PRs. But as a maintainer, you can approve your own PRs.

**Q: What if I need to make an emergency hotfix?**
A: Create a PR even for hotfixes. You can approve and merge immediately, but CI will still run.

**Q: Can I bypass branch protection?**
A: Only repository admins can bypass, and it's logged. Don't do it for rule changes.

**Q: What if CI is broken and I need to merge?**
A: Fix CI first, or temporarily remove the status check requirement (document why in an issue).

## Security Note

**Never** disable branch protection to "save time" on rule changes. The friction is intentional - it's there to make you think twice before weakening quality standards.

> "If weakening rules feels easier than fixing code, you're optimizing for the wrong thing."
