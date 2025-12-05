# Email Templates

This folder contains HTML email templates used by the EmailService.

## Available Templates

### 1. PasswordRecovery.html
Sent when a user requests a password reset.

**Placeholders:**
- `{{Username}}` - User's display name
- `{{ResetLink}}` - Password reset URL with token
- `{{LogoUrl}}` - Company/app logo URL
- `{{AppUrl}}` - Main application URL
- `{{SupportUrl}}` - Support page URL
- `{{Year}}` - Current year (auto-populated)

### 2. PasswordResetConfirmation.html
Sent after a successful password reset.

**Placeholders:**
- `{{Username}}` - User's display name
- `{{LoginUrl}}` - Login page URL
- `{{LogoUrl}}` - Company/app logo URL
- `{{AppUrl}}` - Main application URL
- `{{SupportUrl}}` - Support page URL
- `{{Year}}` - Current year (auto-populated)

## Configuration

Edit `appsettings.json` or `appsettings.Development.json` to configure default placeholder values:

```json
{
  "AppSettings": {
    "LogoUrl": "https://your-cdn.com/logo.png",
    "AppUrl": "https://yourdomain.com",
    "SupportUrl": "https://yourdomain.com/support",
    "LoginUrl": "https://yourdomain.com/login"
  }
}
```

## Customization

### To Update Logo:
1. Host your logo image on a CDN or public URL
2. Update `AppSettings:LogoUrl` in appsettings.json
3. Recommended size: 120x40px or similar aspect ratio

### To Change Colors:
Edit the CSS within each HTML file. Current color palette (matching Home Page):
- Primary: `#2563eb` (Blue 600)
- Primary Gradient End: `#0284c7` (Sky 600)
- Success Light: `#dbeafe` (Blue 100)
- Success: `#bfdbfe` (Blue 200)
- Success Dark: `#1e40af` (Blue 800)
- Warning: `#fef3c7` (Amber 100)
- Warning Accent: `#f59e0b` (Amber 500)
- Link Background: `#eff6ff` (Blue 50)

### To Add New Template:
1. Create a new `.html` file in this folder
2. Use `{{PlaceholderName}}` for dynamic content
3. Call via `_templateService.LoadTemplateAsync("TemplateName", placeholders)`

## Testing

Templates are loaded at runtime - no compilation required. Simply edit the HTML files and restart the application to see changes.

## Best Practices

- Keep templates mobile-responsive
- Use inline CSS for maximum email client compatibility
- Test with multiple email clients (Gmail, Outlook, Apple Mail)
- Avoid complex layouts or JavaScript
- Keep total email size under 100KB
- Always include plain text fallback for accessibility
