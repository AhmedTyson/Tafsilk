# Security Policy

## Supported Versions

Currently supported versions with security updates:

| Version | Supported  |
| ------- | ------------------ |
| 1.0.x | :white_check_mark: |

## Reporting a Vulnerability

We take the security of Tafsilk Platform seriously. If you discover a security vulnerability, please follow these steps:

### 1. **Do NOT** disclose the vulnerability publicly

Please do not create a public GitHub issue for security vulnerabilities.

### 2. Report the vulnerability privately

Send an email to: **security@tafsilk.com** with:

- A description of the vulnerability
- Steps to reproduce the issue
- Potential impact
- Any suggested fixes (optional)

### 3. Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: Depends on severity
  - Critical: Within 7 days
  - High: Within 14 days
  - Medium: Within 30 days
  - Low: Next scheduled release

## Security Best Practices

### For Developers

1. **Never commit secrets**
   - Use User Secrets for local development
   - Use environment variables for production
   - Never commit `appsettings.*.local.json` files

2. **Keep dependencies updated**
   ```bash
   dotnet list package --outdated
   dotnet add package <PackageName> --version <LatestVersion>
   ```

3. **Use parameterized queries**
   - Entity Framework Core already does this
   - Never concatenate SQL strings

4. **Validate all inputs**
   - Use FluentValidation
   - Sanitize user input
   - Implement rate limiting

5. **Follow OWASP Top 10**
   - https://owasp.org/www-project-top-ten/

### For Deployment

1. **Use HTTPS everywhere**
   - Enforce HTTPS in production
   - Use valid SSL certificates
   - Enable HSTS

2. **Secure configuration**
   ```bash
   # Example: Setting secrets in production
   export ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
   export Jwt__Key="YOUR_JWT_SECRET_KEY"
   export Google__client_id="YOUR_GOOGLE_CLIENT_ID"
   export Google__client_secret="YOUR_GOOGLE_CLIENT_SECRET"
   ```

3. **Database security**
   - Use least privilege principle
   - Enable SSL/TLS for database connections
   - Regularly backup databases
   - Encrypt sensitive data at rest

4. **Authentication & Authorization**
   - Use strong password policies
   - Implement account lockout
   - Enable two-factor authentication (planned)
   - Use secure session management

5. **Monitoring & Logging**
   - Enable application insights
 - Monitor failed login attempts
   - Set up alerts for suspicious activity
   - Regular security audits

## Security Features Implemented

✅ **Authentication**
- Cookie-based authentication for web
- JWT Bearer tokens for API
- OAuth 2.0 (Google, Facebook)
- Password hashing with PBKDF2

✅ **Authorization**
- Role-based access control (RBAC)
- Policy-based authorization
- Custom authorization handlers

✅ **Data Protection**
- Anti-forgery tokens
- CORS configuration
- SQL injection prevention (EF Core)
- XSS protection

✅ **Session Management**
- Secure cookie configuration
- Session timeout
- Sliding expiration

✅ **Email Verification**
- Token-based email verification
- Token expiration

## Known Security Considerations

⚠️ **Development Mode**
- Uses HTTP in development (localhost only)
- Swagger UI enabled in development
- Detailed error pages in development

⚠️ **Production Checklist**
- [ ] Change default admin password
- [ ] Set strong JWT secret key
- [ ] Configure OAuth secrets via environment variables
- [ ] Enable HTTPS only
- [ ] Disable detailed errors
- [ ] Configure proper CORS policy
- [ ] Set up application monitoring
- [ ] Regular security updates
- [ ] Database connection string encryption
- [ ] Implement rate limiting
- [ ] Set up Web Application Firewall (WAF)

## Security Hardening

### Recommended Production Settings

**appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DetailedErrors": false,
  "Features": {
    "EnableSwagger": false
  }
}
```

### Headers Security

Add the following middleware in production:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});
```

## Compliance

- **GDPR**: User data handling compliant
- **PCI DSS**: Not storing card data (use payment gateway)
- **Egyptian Data Protection Law**: Compliant

## Contact

For security-related questions or concerns:
- Email: security@tafsilk.com
- Lead Developer: Ahmed Tyson

## Acknowledgments

We appreciate responsible disclosure and will acknowledge security researchers who report vulnerabilities (with permission).

---

Last Updated: January 2025
