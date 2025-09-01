#!/bin/bash

# SmartPlanner Security Tests Runner
# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== SmartPlanner Security Tests ===${NC}"
echo -e "${YELLOW}Starting security test suite...${NC}"

# Create results directory
mkdir -p TestResults/Security

# Build solution first
echo -e "${YELLOW}Building solution...${NC}"
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed. Exiting.${NC}"
    exit 1
fi

# Run security tests
echo -e "${YELLOW}Running JWT security tests...${NC}"
dotnet test tests/SmartPlanner.Tests.Security/JwtSecurityTests.cs --configuration Release --logger "trx;LogFileName=jwt-security-tests.trx" --results-directory TestResults/Security
JWT_RESULT=$?

echo -e "${YELLOW}Running password security tests...${NC}"
dotnet test tests/SmartPlanner.Tests.Security/PasswordSecurityTests.cs --configuration Release --logger "trx;LogFileName=password-security-tests.trx" --results-directory TestResults/Security
PASSWORD_RESULT=$?

echo -e "${YELLOW}Running input validation tests...${NC}"
dotnet test tests/SmartPlanner.Tests.Security/InputValidationTests.cs --configuration Release --logger "trx;LogFileName=input-validation-tests.trx" --results-directory TestResults/Security
INPUT_RESULT=$?

echo -e "${YELLOW}Running HTTPS enforcement tests...${NC}"
dotnet test tests/SmartPlanner.Tests.Security/HttpsEnforcementTests.cs --configuration Release --logger "trx;LogFileName=https-enforcement-tests.trx" --results-directory TestResults/Security
HTTPS_RESULT=$?

# Run all security tests together
echo -e "${YELLOW}Running complete security test suite...${NC}"
dotnet test tests/SmartPlanner.Tests.Security/ --configuration Release --logger "trx;LogFileName=all-security-tests.trx" --results-directory TestResults/Security
ALL_RESULT=$?

# Generate summary report
echo -e "${YELLOW}Generating security test report...${NC}"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
cat > TestResults/Security/security-summary.md << EOF
# Security Test Summary
**Generated:** $TIMESTAMP

## Test Results Overview
EOF

# JWT Security Tests
if [ $JWT_RESULT -eq 0 ]; then
    echo -e "✅ **JWT Security Tests:** PASSED" >> TestResults/Security/security-summary.md
    echo -e "${GREEN}✅ JWT Security Tests: PASSED${NC}"
else
    echo -e "❌ **JWT Security Tests:** FAILED" >> TestResults/Security/security-summary.md
    echo -e "${RED}❌ JWT Security Tests: FAILED${NC}"
fi

# Password Security Tests
if [ $PASSWORD_RESULT -eq 0 ]; then
    echo -e "✅ **Password Security Tests:** PASSED" >> TestResults/Security/security-summary.md
    echo -e "${GREEN}✅ Password Security Tests: PASSED${NC}"
else
    echo -e "❌ **Password Security Tests:** FAILED" >> TestResults/Security/security-summary.md
    echo -e "${RED}❌ Password Security Tests: FAILED${NC}"
fi

# Input Validation Tests
if [ $INPUT_RESULT -eq 0 ]; then
    echo -e "✅ **Input Validation Tests:** PASSED" >> TestResults/Security/security-summary.md
    echo -e "${GREEN}✅ Input Validation Tests: PASSED${NC}"
else
    echo -e "❌ **Input Validation Tests:** FAILED" >> TestResults/Security/security-summary.md
    echo -e "${RED}❌ Input Validation Tests: FAILED${NC}"
fi

# HTTPS Enforcement Tests
if [ $HTTPS_RESULT -eq 0 ]; then
    echo -e "✅ **HTTPS Enforcement Tests:** PASSED" >> TestResults/Security/security-summary.md
    echo -e "${GREEN}✅ HTTPS Enforcement Tests: PASSED${NC}"
else
    echo -e "❌ **HTTPS Enforcement Tests:** FAILED" >> TestResults/Security/security-summary.md
    echo -e "${RED}❌ HTTPS Enforcement Tests: FAILED${NC}"
fi

cat >> TestResults/Security/security-summary.md << EOF

## Security Checklist
- [x] JWT token validation and tampering protection
- [x] BCrypt password hashing verification
- [x] SQL injection prevention testing
- [x] XSS payload sanitization testing
- [x] HTTPS redirection and security headers
- [x] Input validation and length limits
- [x] Path traversal protection
- [x] Brute force protection testing

## Recommendations
1. Regularly update security dependencies
2. Monitor for new security vulnerabilities
3. Implement additional rate limiting if needed
4. Consider adding CAPTCHA for login attempts
5. Review and update security headers periodically
EOF

# List generated files
echo -e "\n${BLUE}Generated Files:${NC}"
find TestResults/Security -name "*.trx" -o -name "*.md" | while read file; do
    echo -e "${GREEN}- $file${NC}"
done

# Final status
TOTAL_TESTS=4
PASSED_TESTS=0
[ $JWT_RESULT -eq 0 ] && ((PASSED_TESTS++))
[ $PASSWORD_RESULT -eq 0 ] && ((PASSED_TESTS++))
[ $INPUT_RESULT -eq 0 ] && ((PASSED_TESTS++))
[ $HTTPS_RESULT -eq 0 ] && ((PASSED_TESTS++))

echo -e "\n${BLUE}Security Test Summary: $PASSED_TESTS/$TOTAL_TESTS tests passed${NC}"

if [ $ALL_RESULT -eq 0 ]; then
    echo -e "${GREEN}🔒 All security tests completed successfully!${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠️  Some security tests had issues. Check the results above.${NC}"
    exit 1
fi
