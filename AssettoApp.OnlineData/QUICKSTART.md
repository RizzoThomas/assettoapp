# Quick Start: Online Setup Providers

## Overview

AssettoApp can now fetch racing setups from online sources! Since RaceDepartment and Setup Market don't provide public APIs, the system uses a flexible web scraping approach that you can configure.

## How It Works

1. **No official APIs found**: After research, no public APIs were available from popular setup sites
2. **Configurable scraping**: The system can scrape data from any website you configure
3. **Automatic fallback**: If online data is unavailable, local presets are used
4. **Rate limiting**: Automatic rate limiting to be respectful to websites

## For Users: Getting Started

### Step 1: Configuration File Location

The application creates a configuration file at:
```
Windows: %APPDATA%\AssettoApp\setup_sources.json
```

This file is created automatically with example configurations on first run.

### Step 2: Understanding the Configuration

The system needs to know:
- Where to search (URL pattern)
- How to extract data (XPath selectors)
- Rate limiting settings

Example configuration structure:
```json
{
  "SourceName": "My Setup Database",
  "BaseUrl": "https://example.com",
  "SearchUrlPattern": "https://example.com/search?q={car}+{track}",
  "SetupItemSelector": "//div[@class='setup-item']",
  "TitleSelector": ".//h3[@class='title']",
  "AuthorSelector": ".//span[@class='author']",
  "DownloadLinkSelector": ".//a[@class='download']/@href",
  "MinRequestDelayMs": 2000,
  "MaxRequestsPerMinute": 20,
  "IsEnabled": true
}
```

### Step 3: Provide Setup Database URLs

**Option A: Use Pre-configured Sites (if they work)**
- Edit the JSON file
- Set `"IsEnabled": true` for RaceDepartment or Setup Market
- Test to see if data is retrieved

**Option B: Configure Your Own Sources**

If you know of other setup databases:

1. Find a website with Assetto Corsa setups
2. Open it in your browser and search for a setup
3. Note the URL pattern and HTML structure
4. Use browser DevTools (F12) to inspect the HTML
5. Find XPath selectors for the data you want to extract
6. Add a new configuration to the JSON file

### Step 4: Finding XPath Selectors

In Chrome/Edge:
1. Right-click on an element → "Inspect"
2. In DevTools, right-click the HTML element
3. Select "Copy" → "Copy XPath"
4. Simplify the XPath to be more generic

Example: Instead of `/html/body/div[1]/div[2]/div[3]`
Use: `//div[@class='setup-card']`

### Step 5: Test Your Configuration

1. Save the configuration file
2. Restart AssettoApp
3. Select "Game Closed" mode
4. Choose a car and track
5. Click "Generate Setup (Online)"
6. Check the status message to see if data was retrieved

## Current Status in UI

When you generate a setup in Online Analysis mode, the application will show:

- **Available providers**: Which sources are configured and working
- **Data sources used**: What data was actually used
- **Confidence score**: How reliable the setup is
- **Fallback status**: If local data was used instead

Example output:
```
Data Sources:
• Online setup databases (active):
  - RaceDepartment: ✓ Available
  - Custom Source: ✗ Unavailable
• Local presets and physics models
• Track characteristics database

Notes:
• Using setup from RaceDepartment
```

## Important Notes

### What This System Does
- ✅ Extracts metadata (title, author, description, ratings)
- ✅ Provides download links
- ✅ Respects rate limits
- ✅ Falls back to local data gracefully

### What It Doesn't Do (Yet)
- ❌ Download actual setup files automatically
- ❌ Parse setup file formats
- ❌ Bypass CAPTCHA or anti-bot measures
- ❌ Handle authentication flows

## Example: Configuring a Custom Forum

Suppose you have access to a forum with this HTML structure:

```html
<div class="forum-post">
  <h2 class="post-title">Ferrari 488 GT3 @ Spa Setup</h2>
  <span class="author">John Doe</span>
  <div class="post-content">
    <p>My fastest setup for dry conditions...</p>
    <a href="/download/123">Download Setup</a>
  </div>
</div>
```

Your configuration would be:

```json
{
  "SourceName": "My Racing Forum",
  "BaseUrl": "https://myforum.com",
  "SearchUrlPattern": "https://myforum.com/search?q={car}+{track}+setup",
  "SetupItemSelector": "//div[@class='forum-post']",
  "TitleSelector": ".//h2[@class='post-title']",
  "AuthorSelector": ".//span[@class='author']",
  "DownloadLinkSelector": ".//a[contains(text(), 'Download')]/@href",
  "DescriptionSelector": ".//div[@class='post-content']/p",
  "MinRequestDelayMs": 3000,
  "MaxRequestsPerMinute": 15,
  "IsEnabled": true
}
```

## Troubleshooting

### No data retrieved
- Check if `IsEnabled` is `true`
- Verify the website is accessible
- Test the search URL in a browser
- Website structure may have changed - update selectors

### Wrong data extracted
- XPath selectors may be incorrect
- Use browser DevTools to verify element structure
- Test XPath in browser console: `$x("//div[@class='setup-card']")`

### Connection errors
- Check internet connection
- Some sites block automated access
- CAPTCHA may be preventing access
- Consider contacting site owners for permission

## Best Practices

1. **Be Respectful**
   - Use reasonable rate limits (2-3 seconds minimum)
   - Don't overload websites
   - Consider asking website owners for permission

2. **Keep Updated**
   - Website structures change over time
   - Update selectors when needed
   - Test periodically

3. **Share Configurations**
   - If you create a working configuration, share it!
   - Submit via GitHub issues or pull requests
   - Help the community

## For Developers: Extending the System

See `README_PROVIDERS.md` for detailed technical documentation.

Key files:
- `Models/SetupSourceConfiguration.cs` - Configuration model
- `Providers/ConfigurableScrapingProvider.cs` - Main scraping logic
- `Utilities/RateLimiter.cs` - Rate limiting implementation
- `Utilities/SetupSourceConfigManager.cs` - Configuration management

## Future Enhancements

Planned features:
- Direct setup file download
- Setup file format parsing (.ini, .json)
- Built-in configurations for popular sites
- API integration when APIs become available
- Community repository of working configurations

## Need Help?

1. Read the full documentation in `README_PROVIDERS.md`
2. Check example configurations in `setup_sources.example.json`
3. Open an issue on GitHub with:
   - Your configuration (remove any API keys!)
   - Error messages
   - What you're trying to achieve

## Legal Notice

- Respect website Terms of Service
- Web scraping may be against some sites' policies
- Use at your own risk
- Always prefer official APIs when available
- Consider copyright and data ownership

---

**Summary**: The online provider system is now functional and ready to use! Configure your data sources, and the application will automatically try to fetch setup information while falling back to local presets when needed.
