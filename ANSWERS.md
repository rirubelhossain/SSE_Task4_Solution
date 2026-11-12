# Task 4 — written solution

## 1. What are the goals of HTTPS and how are they achieved?
HTTPS protects confidentiality (others cannot read the traffic), integrity (unauthorized changes are detected), and server authentication (the client verifies the server identity). It achieves these through HTTP over TLS. A TLS handshake authenticates the server using its certificate and establishes shared keys; authenticated encryption protects the exchanged data. Client certificate authentication is optional. HTTPS does not by itself guarantee that a website's content is trustworthy.

## 2. What differs between HTTP and HTTPS request/response messages?
The HTTP methods, status codes, headers and body retain their meaning. With HTTPS, HTTP data is carried inside a TLS-protected connection; with plain HTTP it is transmitted without that TLS protection. An HTTP/1.1 request does not acquire an “HTTPS/1.1” start line. Common default ports are 80 for HTTP and 443 for HTTPS. The visible wire representation can also differ by negotiated HTTP version; the exercise uses HTTP/1.1 text messages.

## 3. What is the purpose of HTTP cookies?
Cookies let a browser remember state across separate HTTP requests, for example a login session identifier, preferences or this visit count. A server sends Set-Cookie; the browser stores the cookie and sends it in Cookie on later matching requests, subject to its scope and browser policy. HTTP itself does not automatically remember earlier requests.

## 4. Completed exchange diagram
Each response is 200 OK. The first request has no visits cookie. A browser may send unrelated cookies, which are omitted here. Blank lines separate headers from bodies. For clarity, Content-Length is left out of the diagram; the implementation calculates it and closes the connection after each response.

First request:
```http
GET /index.html HTTP/1.1
Host: example.org

```
First response:
```http
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Cache-Control: no-store
Connection: close
Set-Cookie: visits=1; Path=/; HttpOnly; SameSite=Lax

This is your 1. visit
```
Second request:
```http
GET /index.html HTTP/1.1
Host: example.org
Cookie: visits=1

```
Second response:
```http
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Cache-Control: no-store
Connection: close
Set-Cookie: visits=2; Path=/; HttpOnly; SameSite=Lax

This is your 2. visit
```
Third request:
```http
GET /index.html HTTP/1.1
Host: example.org
Cookie: visits=2

```
Third response:
```http
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Cache-Control: no-store
Connection: close
Set-Cookie: visits=3; Path=/; HttpOnly; SameSite=Lax

This is your 3. visit
```
For a real HTTPS deployment, add Secure to Set-Cookie and configure TLS. Do not add Secure to this plain HTTP teaching example and assume TLS is thereby enabled.

## 5. Template implementation
SetCookie builds the response Set-Cookie header. GetCookies reads semicolon-separated name/value pairs from the request Cookie header, splitting at the first equals sign. HttpServer.ReceiveRequest reads visits, treats a missing/invalid/negative/overflowing value as zero, increments once, returns the HTML representation, and sets the new cookie value.

Example: received Cookie: visits=2; server computes previous=2 and current=3; response body shows visit 3 and prior visits 2; Set-Cookie stores visits=3.

The browser is the client in this exercise. There is no Client.cs to start separately. Keep the console server running while you refresh the browser.
