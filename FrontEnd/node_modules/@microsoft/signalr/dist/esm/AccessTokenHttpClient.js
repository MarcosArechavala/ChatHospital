// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
import { HeaderNames } from "./HeaderNames";
import { HttpClient } from "./HttpClient";
/** @private */
export class AccessTokenHttpClient extends HttpClient {
    constructor(innerClient, accessTokenFactory) {
        super();
        this._refreshRequestTokens = new WeakMap();
        this._innerClient = innerClient;
        this._accessTokenFactory = accessTokenFactory;
        this._refreshAccessTokenFactory = accessTokenFactory;
    }
    async send(request) {
        let allowRetry = true;
        const accessTokenRequest = request;
        const isNegotiate = accessTokenRequest._negotiate === true;
        const isRefresh = accessTokenRequest._authenticationRefresh === true;
        let accessToken = isRefresh ? undefined : this._accessToken;
        if (isRefresh && this._refreshAccessTokenFactory) {
            // Refresh belongs to the application auth plane. Do not overwrite the cached transport token until the server accepts the refresh.
            allowRetry = false;
            accessToken = await this._refreshAccessTokenFactory();
        }
        else if (this._accessTokenFactory && (!this._accessToken || isNegotiate)) {
            // don't retry if the request is a negotiate or if we just got a potentially new token from the access token factory
            allowRetry = false;
            accessToken = await this._accessTokenFactory();
            this._accessToken = accessToken;
        }
        this._setAuthorizationHeader(request, accessToken);
        const response = await this._innerClient.send(request);
        if (isRefresh) {
            this._refreshRequestTokens.set(response, accessToken);
        }
        if (allowRetry && response.statusCode === 401 && this._accessTokenFactory) {
            this._accessToken = await this._accessTokenFactory();
            this._setAuthorizationHeader(request, this._accessToken);
            return await this._innerClient.send(request);
        }
        return response;
    }
    markNegotiateRequest(request) {
        request._negotiate = true;
    }
    markAuthenticationRefreshRequest(request) {
        request._authenticationRefresh = true;
    }
    setRefreshAccessTokenFactory(accessTokenFactory) {
        this._refreshAccessTokenFactory = accessTokenFactory;
    }
    updateCachedToken(accessToken) {
        this._accessToken = accessToken;
    }
    getRefreshRequestToken(response) {
        return this._refreshRequestTokens.get(response);
    }
    _setAuthorizationHeader(request, accessToken) {
        if (!request.headers) {
            request.headers = {};
        }
        if (accessToken) {
            request.headers[HeaderNames.Authorization] = `Bearer ${accessToken}`;
        }
        // don't remove the header if there isn't an access token factory, the user manually added the header in this case
        else if (this._accessTokenFactory) {
            if (request.headers[HeaderNames.Authorization]) {
                delete request.headers[HeaderNames.Authorization];
            }
        }
    }
    getCookieString(url) {
        return this._innerClient.getCookieString(url);
    }
}
//# sourceMappingURL=AccessTokenHttpClient.js.map