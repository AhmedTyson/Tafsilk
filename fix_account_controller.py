#!/usr/bin/env python3
"""
Script to fix AccountController.cs by:
1. Removing duplicate methods
2. Adding missing Settings action
3. Adding password reset actions
"""

import re

# Read the file
with open(r"C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web\Controllers\AccountController.cs", "r", encoding="utf-8") as f:
    content = f.read()

# Find and remove the duplicate VerifyEmail and ResendVerificationEmail methods
# These are the second occurrences (around line 1100+)

# Split by method signatures to identify duplicates
lines = content.split('\n')

# Find first occurrence of VerifyEmail
first_verify_email = -1
second_verify_email = -1
first_resend = -1
second_resend = -1

for i, line in enumerate(lines):
    if 'public async Task<IActionResult> VerifyEmail(string token)' in line:
        if first_verify_email == -1:
    first_verify_email = i
 else:
         second_verify_email = i
    
    if 'public IActionResult ResendVerificationEmail()' in line and '[HttpGet]' in lines[i-1]:
 if first_resend == -1:
          first_resend = i
        else:
        second_resend = i

print(f"First VerifyEmail at line: {first_verify_email}")
print(f"Second VerifyEmail at line: {second_verify_email}")
print(f"First ResendVerificationEmail at line: {first_resend}")
print(f"Second ResendVerificationEmail at line: {second_resend}")

# Find the duplicate CompleteTailorProfile with [Authorize(Policy = "TailorPolicy")]
duplicate_complete_tailor = -1
for i, line in enumerate(lines):
    if '[Authorize(Policy = "TailorPolicy")]' in line and i > 1200:
        duplicate_complete_tailor = i
        break

print(f"Duplicate CompleteTailorProfile at line: {duplicate_complete_tailor}")
