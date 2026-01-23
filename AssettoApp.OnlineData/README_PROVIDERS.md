# Online Setup Providers - User Guide

## Overview

AssettoApp can fetch racing setups from online databases to enhance setup generation. This document explains how to configure and use online setup providers.

## Architecture

The system consists of:

1. **Built-in Provider Framework**: Placeholder providers for RaceDepartment and Setup Market
2. **Configurable Scraping Provider**: Flexible provider that can scrape data from any website you configure
3. **Rate Limiting**: Automatic rate limiting to respect website policies
4. **Fallback System**: Local preset data is always available as fallback

## Configuration File

Setup sources are configured in:
```
%APPDATA%\AssettoApp\setup_sources.json
```

The application creates example configurations on first run.

## Configuring a Setup Source

### Configuration Structure

```json
{
  "SourceName": "My Setup Database",
  "BaseUrl": "https://example.com",
  "SearchUrlPattern": "https://example.com/setups?car={car}&track={track}",
  "SetupItemSelector": "//div[@class='setup-item']",
  "TitleSelector": ".//h3[@class='title']",
  "AuthorSelector": ".//span[@class='author']",
  "DownloadLinkSelector": ".//a[@class='download']/@href",
  "DescriptionSelector": ".//p[@class='description']",
  "RatingSelector": ".//span[@class='rating']",
  "DownloadCountSelector": ".//span[@class='downloads']",
  "MinRequestDelayMs": 2000,
  "MaxRequestsPerMinute": 20,
  "RequiresAuthentication": false,
  "ApiKey": null,
  "IsEnabled": true
}
```

### Field Descriptions

| Field | Description | Example |
|-------|-------------|---------|
| `SourceName` | Display name for this source | "RaceDepartment" |
| `BaseUrl` | Base URL of the website | "https://www.racedepartment.com" |
| `SearchUrlPattern` | URL pattern for searches. Use `{game}`, `{car}`, `{track}` placeholders | "https://example.com/search?q={car}+{track}" |
| `SetupItemSelector` | XPath selector for individual setup items in search results | "//div[@class='setup-card']" |
| `TitleSelector` | XPath selector for setup title (relative to item) | ".//h3[@class='title']" |
| `AuthorSelector` | XPath selector for author name | ".//span[@class='author']" |
| `DownloadLinkSelector` | XPath selector for download link | ".//a[@class='download']/@href" |
| `DescriptionSelector` | XPath selector for description/notes | ".//p[@class='notes']" |
| `RatingSelector` | XPath selector for rating | ".//span[@class='rating']" |
| `DownloadCountSelector` | XPath selector for download count | ".//span[@class='downloads']" |
| `MinRequestDelayMs` | Minimum milliseconds between requests | 2000 |
| `MaxRequestsPerMinute` | Maximum requests per minute | 20 |
| `RequiresAuthentication` | Whether API key is needed | false |
| `ApiKey` | API key if required | null |
| `IsEnabled` | Whether to use this source | true |

### XPath Selectors

The configuration uses XPath selectors to extract data from HTML:

- Start with `//` for absolute paths: `//div[@class='setup']`
- Start with `.//` for relative paths (within an item): `.//h3[@class='title']`
- Use `/@href` to select attribute values: `.//a[@class='link']/@href`

#### Finding Selectors

1. Open the website in Chrome/Edge
2. Right-click on an element → "Inspect"
3. In DevTools, right-click the HTML element → "Copy" → "Copy XPath"
4. Simplify the XPath to be more generic (replace specific IDs with classes if possible)

### Example: Configuring for a Custom Website

Suppose you have a website with this structure:

```html
<div class="setup-list">
  <div class="setup-card">
    <h2 class="setup-name">Ferrari 488 GT3 - Spa Setup</h2>
    <span class="author-name">John Doe</span>
    <a href="/download/123" class="btn-download">Download</a>
    <p class="setup-notes">Fast setup for dry conditions</p>
    <div class="stats">
      <span class="rating">4.5</span>
      <span class="downloads">1234</span>
    </div>
  </div>
</div>
```

Your configuration would be:

```json
{
  "SourceName": "Custom Setup Site",
  "BaseUrl": "https://customsite.com",
  "SearchUrlPattern": "https://customsite.com/search?q={car}+{track}",
  "SetupItemSelector": "//div[@class='setup-card']",
  "TitleSelector": ".//h2[@class='setup-name']",
  "AuthorSelector": ".//span[@class='author-name']",
  "DownloadLinkSelector": ".//a[@class='btn-download']/@href",
  "DescriptionSelector": ".//p[@class='setup-notes']",
  "RatingSelector": ".//div[@class='stats']//span[@class='rating']",
  "DownloadCountSelector": ".//div[@class='stats']//span[@class='downloads']",
  "MinRequestDelayMs": 2000,
  "MaxRequestsPerMinute": 20,
  "IsEnabled": true
}
```

## Rate Limiting

The system automatically enforces rate limits to be respectful to websites:

- **MinRequestDelayMs**: Minimum time between any two requests
- **MaxRequestsPerMinute**: Maximum number of requests in a 60-second window

Both limits are enforced simultaneously. For example:
- MinRequestDelayMs = 2000 means at least 2 seconds between requests
- MaxRequestsPerMinute = 20 means maximum 20 requests per minute

### Recommended Settings

| Website Type | MinRequestDelayMs | MaxRequestsPerMinute |
|--------------|-------------------|----------------------|
| Public community site | 2000-3000 | 15-20 |
| Personal/small site | 3000-5000 | 10-15 |
| Commercial API | 1000-2000 | 30-60 |

## Testing Your Configuration

1. Edit `setup_sources.json`
2. Restart AssettoApp
3. Select "Game Closed" mode
4. Choose a car and track
5. Click "Generate Setup (Online)"
6. Check status message for results

## Troubleshooting

### No Data Retrieved

1. **Check if source is enabled**: `"IsEnabled": true` in config
2. **Verify URL pattern**: Test the search URL in a browser
3. **Check selectors**: Use browser DevTools to verify XPath selectors
4. **Rate limiting**: May need to wait between attempts

### Incorrect Data Extracted

1. **Inspect HTML structure**: Website structure may have changed
2. **Update selectors**: Adjust XPath selectors to match current structure
3. **Test with simple selector**: Start with basic selector and refine

### Connection Errors

1. **Check internet connection**
2. **Verify website is accessible**
3. **Check for CAPTCHA or anti-bot measures**
4. **Some sites may block automated access**

## Best Practices

### Be Respectful

- Use reasonable rate limits
- Don't overload websites with requests
- Consider contacting website owners for permission

### Keep Config Updated

- Website structures change
- Update selectors when needed
- Test periodically

### Security

- Don't share API keys publicly
- Use secure HTTPS URLs only
- Be cautious with authentication

## Example Configurations

### RaceDepartment (Example - May Need Updates)

```json
{
  "SourceName": "RaceDepartment",
  "BaseUrl": "https://www.racedepartment.com",
  "SearchUrlPattern": "https://www.racedepartment.com/downloads/categories/ac-setups.23/?q={car}+{track}",
  "SetupItemSelector": "//div[contains(@class, 'structItem')]",
  "TitleSelector": ".//h3[@class='structItem-title']//a",
  "AuthorSelector": ".//a[contains(@class, 'username')]",
  "DownloadLinkSelector": ".//h3[@class='structItem-title']//a/@href",
  "MinRequestDelayMs": 2000,
  "MaxRequestsPerMinute": 20,
  "IsEnabled": false
}
```

### Custom Community Forum

```json
{
  "SourceName": "Community Forum",
  "BaseUrl": "https://example-forum.com",
  "SearchUrlPattern": "https://example-forum.com/forum/search.php?keywords={car}+{track}+setup",
  "SetupItemSelector": "//div[@class='post']",
  "TitleSelector": ".//h4[@class='post-title']",
  "AuthorSelector": ".//span[@class='author']",
  "DescriptionSelector": ".//div[@class='content']",
  "MinRequestDelayMs": 3000,
  "MaxRequestsPerMinute": 15,
  "IsEnabled": true
}
```

## API Integration (Future)

If a website provides an official API:

1. The API key can be configured in `ApiKey` field
2. Set `RequiresAuthentication: true`
3. The provider will include the key in requests
4. API endpoints would need custom provider implementation

## Limitations

### Current Limitations

1. **No automatic authentication**: Manual API keys only
2. **HTML structure dependent**: Scraping breaks if site changes
3. **No CAPTCHA bypass**: Sites with CAPTCHA won't work
4. **Limited data extraction**: Only metadata, not actual setup files

### Future Enhancements

- Direct setup file download and parsing
- API integrations with official endpoints
- Authentication flows (OAuth, etc.)
- Caching and offline mode
- Setup file format conversion

## Privacy and Legal

- Respect website Terms of Service
- Don't violate copyright
- Scraping may be against some sites' policies
- Use at your own risk
- Consider official APIs when available

## Support

For issues or questions:

1. Check this documentation
2. Verify your configuration
3. Test with browser DevTools
4. Open an issue on GitHub with:
   - Your configuration (remove API keys)
   - Error messages
   - Website you're trying to scrape

## Contributing

To add support for a popular setup database:

1. Test your configuration thoroughly
2. Document the selectors
3. Submit a pull request with example config
4. Include testing instructions
