import { HttpClient, HttpRequest, HttpResponse } from "./HttpClient";
/** @private */
export declare class AccessTokenHttpClient extends HttpClient {
    private _innerClient;
    private _refreshAccessTokenFactory;
    private readonly _refreshRequestTokens;
    _accessToken: string | undefined;
    _accessTokenFactory: (() => string | Promise<string>) | undefined;
    constructor(innerClient: HttpClient, accessTokenFactory: (() => string | Promise<string>) | undefined);
    send(request: HttpRequest): Promise<HttpResponse>;
    markNegotiateRequest(request: HttpRequest): void;
    markAuthenticationRefreshRequest(request: HttpRequest): void;
    setRefreshAccessTokenFactory(accessTokenFactory: (() => string | Promise<string>) | undefined): void;
    updateCachedToken(accessToken: string | undefined): void;
    getRefreshRequestToken(response: HttpResponse): string | undefined;
    private _setAuthorizationHeader;
    getCookieString(url: string): string;
}
