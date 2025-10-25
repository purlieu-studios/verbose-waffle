## Summary

<!-- Provide a clear, concise summary of what this PR does and WHY it's needed -->

### What changed?

<!-- Bullet points describing the specific changes made -->
-

### Why was this change necessary?

<!-- Explain the problem this solves or the feature this enables -->


### Related Issues

<!-- Link to related issues, tickets, or discussions -->
Fixes #
Relates to #

---

## Type of Change

<!-- Check all that apply -->

- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📝 Documentation update
- [ ] ♻️ Refactoring (no functional changes)
- [ ] 🎨 Style/formatting changes
- [ ] ⚡ Performance improvement
- [ ] ✅ Test additions or updates
- [ ] 🔧 Configuration changes
- [ ] 🔒 Security fix

---

## Testing & Verification

### How was this tested?

<!-- Describe the testing you performed to verify your changes -->

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed
- [ ] Tested in Godot editor
- [ ] Tested in game runtime

### Test Coverage

<!-- What scenarios were tested? -->
- [ ] Happy path scenarios
- [ ] Edge cases
- [ ] Error handling
- [ ] Null/empty input handling
- [ ] Performance under load (if applicable)

### Testing Evidence

<!-- Provide screenshots, logs, or other evidence of testing -->

```
Paste test output, screenshots, or recordings here
```

---

## Code Quality Checklist

<!-- These are enforced by CI - make sure they all pass -->

- [ ] ✅ Code builds without errors
- [ ] ✅ All tests pass
- [ ] ✅ No new compiler warnings
- [ ] ✅ Code analysis rules pass (no new violations)
- [ ] ✅ Follows naming conventions (see `.editorconfig`)
- [ ] ✅ No commented-out code or debug statements
- [ ] ✅ No TODO comments without issue references
- [ ] ✅ Nullable reference types handled correctly

### If Code Quality Rules Were Modified

**⚠️ STOP! Read this carefully:**

- [ ] I have read `CODE_QUALITY_RULES.md`
- [ ] This change is documented in `CODE_QUALITY_RULES.md` with rationale
- [ ] I tried to fix the code first before modifying rules
- [ ] This change has been discussed with the team
- [ ] I understand this will trigger extra scrutiny in review

**Why are you modifying code quality rules?**

<!-- If you checked any rule modification boxes above, explain in detail -->

```
[Detailed explanation required]
```

---

## Documentation

- [ ] Code comments added/updated for complex logic
- [ ] Public APIs documented with XML comments
- [ ] README updated (if public-facing changes)
- [ ] Migration guide provided (if breaking changes)
- [ ] CHANGELOG updated (if applicable)

---

## Security Considerations

<!-- Consider security implications of your changes -->

- [ ] No sensitive data (passwords, keys, tokens) in code
- [ ] Input validation added where necessary
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities (if web-related)
- [ ] Dependencies are trusted and up-to-date
- [ ] Secrets use secure storage (not hardcoded)

**Security Impact:** None / Low / Medium / High

<!-- If Medium or High, explain: -->

---

## Performance Impact

<!-- Consider performance implications -->

- [ ] No performance regression
- [ ] Performance improvement (provide metrics)
- [ ] Performance impact acceptable for feature value
- [ ] Not applicable

**Performance Notes:**

<!-- Provide benchmarks, profiling data, or analysis if relevant -->

---

## Godot-Specific Considerations

<!-- For Godot/game development changes -->

- [ ] Works in both editor and runtime
- [ ] No console errors or warnings in Godot
- [ ] Resources properly managed (no memory leaks)
- [ ] Signals connected/disconnected correctly
- [ ] Scenes can be instantiated without errors
- [ ] Export works correctly (if applicable)
- [ ] Not applicable (non-Godot changes)

---

## Breaking Changes

<!-- If this is a breaking change, document migration path -->

### What breaks?

<!-- List what existing code/functionality will break -->

### Migration Instructions

<!-- Step-by-step guide for users to migrate -->

1.
2.
3.

---

## Screenshots / Recordings

<!-- Visual changes should include before/after screenshots or video -->

### Before


### After


---

## Deployment Notes

<!-- Anything special needed for deployment? -->

- [ ] Database migrations required
- [ ] Configuration changes required
- [ ] Dependencies need to be updated (`dotnet restore`)
- [ ] Environment variables need to be set
- [ ] No special deployment steps needed

**Deployment Instructions:**

<!-- If checked any boxes above, provide details -->

---

## Reviewer Guidance

<!-- Help reviewers understand what to focus on -->

### What should reviewers focus on?

<!-- Highlight areas that need extra attention -->
-

### Known Limitations

<!-- Be honest about what this PR doesn't solve -->
-

### Future Work

<!-- What follow-up work is planned? -->
-

---

## Pre-Merge Checklist

<!-- Final checks before merging -->

- [ ] PR title is clear and follows convention
- [ ] All CI checks pass
- [ ] No merge conflicts
- [ ] Branch is up to date with base branch
- [ ] Code has been self-reviewed
- [ ] Changes have been tested locally
- [ ] Documentation is complete
- [ ] Ready to merge

---

## Additional Context

<!-- Any other information that would help reviewers -->


---

<details>
<summary>📋 PR Template Meta</summary>

**Why this template is so detailed:**

This template enforces good practices by making you think through:
1. **Why** the change is needed (not just what changed)
2. **Testing** - did you actually verify it works?
3. **Code quality** - does it meet our standards?
4. **Security** - are there vulnerabilities?
5. **Documentation** - can others understand/use this?
6. **Breaking changes** - will this break existing code?

**You don't need to fill out every section** - only what's relevant to your PR. Delete sections that don't apply.

**If you're tempted to skip sections:** Ask yourself if you're rushing and might introduce bugs.

</details>
