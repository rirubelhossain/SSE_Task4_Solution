# SSE — Task 4: HTTPS and cookies

The uploaded OPAL sheet is titled Tutorial 03 — URL & HTTP. This solution completes its Task 4 (questions 1–5) using Task4-Template.zip. It does not claim to solve a different Tutorial 04 sheet.

## What is your task?
Build a small website that remembers visits using a cookie. First visit: 1. Refresh: 2. Refresh again: 3. Your browser stores a number and returns it with the next request.

## Open and run in Visual Studio 2022
1. Extract this ZIP (do not open the solution inside the ZIP).
2. Open Task4-Template.sln in Visual Studio 2022 with .NET 8 SDK support.
3. Press Ctrl+F5. Keep the console open: it is your running web server.
4. Open http://127.0.0.1:3000/index.html in your browser.
5. You should see “This is your 1. visit”. Refresh for visits 2 and 3.
6. Close the server with Ctrl+C in its console, or stop it in Visual Studio.

If a number already exists, delete the visits cookie in browser developer tools (Application > Cookies), or use a fresh private browsing session. Private tabs may share cookies within the same private session. Use the same hostname throughout: localhost and 127.0.0.1 have separate cookie stores.

If port 3000 is occupied, stop your earlier server or change the port in Program.cs and in your browser URL. The .NET desktop development workload can be installed through Visual Studio Installer if C# support is missing.

## Files to understand, in this order
- Program.cs starts the server on port 3000.
- HttpServer.cs reads the visits cookie, increments it, creates HTML and sets the response cookie.
- HttpMessage.cs parses/builds HTTP text; SetCookie and GetCookies implement the template TODOs.
- TcpServer.cs receives a complete HTTP header and sends the response.
- Url.cs is retained from the supplied template; the counter uses the request path directly.
- ANSWERS.md contains all written answers and the completed HTTP exchange.

## How the counter works
No cookie: previous = 0, current = 1. Server returns HTML with visit 1 and Set-Cookie: visits=1. Browser remembers it. On refresh the browser sends Cookie: visits=1. Server returns visit 2 and Set-Cookie: visits=2. Only GET / and GET /index.html increment it; favicon requests do not.

The cookie is a session cookie (no Max-Age/Expires). The example sets Path=/, HttpOnly and SameSite=Lax. Secure is omitted because the local example uses HTTP. The server does not implement TLS: the HTTPS questions are answered separately.

The assignment wording mentions prior requests, while its diagram labels the current visit 1, 2, 3. The page displays BOTH current visit and number of prior visits (0, 1, 2), resolving that difference.

## Run the included checks
Open a terminal in the extracted project folder:

    dotnet run --project Task4-Template.csproj -- --test

The checks use port 13004 and exercise real HTTP exchanges for three visits, independent browser state, invalid cookies, missing resources, rejected POST, fragmented TCP headers, response parsing and UTF-8 lengths. Expected final line: ALL CHECKS PASSED.

Validation: source reviewed; automated checks included but not executed in the authoring environment because a .NET SDK/runtime is unavailable. Verify locally using the command above or run the browser demonstration.

## Scope and changes
The original .NET 6 project targets .NET 8, matching the previous exercise. The original class names and solution are retained. Beyond the cookie TODOs, the HTTP helper now uses CRLF, UTF-8 byte lengths, case-insensitive headers, correct response identification and multiword reason phrases. The transport reads to the end of headers instead of assuming one read is a complete request.

This sequential local teaching server supports the GET counter scenario. It is not a complete HTTP implementation: it does not process request bodies, chunked transfer encoding, persistent connections or multiple response cookies. SetCookie on a response replaces its single Set-Cookie value, which is sufficient for the one counter cookie. A client can edit its own count; it is not an authoritative analytics/security counter. Concurrent tabs may send the same previous value.
