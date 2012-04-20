### vow

OAuth 2.0 middleware for OWIN

Responds to a 401 status message returned from a downstream middleware or application by
redirecting to a compatible OAuth 2.0 provider.

#### Configuration

ClientId, ClientSecret, AuthorizeEndpoint, TokenEndpoint, CookieName

#### Token Callback

Configure a Token Callback to fetch user information and setup User context.

#### Tested OAuth Providers
* [GitHub](http://developer.github.com/v3/oauth/)