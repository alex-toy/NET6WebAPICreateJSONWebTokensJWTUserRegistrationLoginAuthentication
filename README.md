# .NET 6 Web API Create JSON Web Tokens (JWT) - User Registration / Login / Authentication

In this project we will create JSON Web Tokens, User Registration, Login and Authentication in our  .NET 6 Web API.

<img src="/pictures/init.png" title="app we're starting with"  width="800">


## Project Start up

- Packages to install
```
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Identity.UI
Microsoft.EntityFrameworkCore.SqlServer
```

- In **Package Manager Console** :
```
Add-Migration init
Update-Database
```


## Register

- First time with a user :

<img src="/pictures/register.png" title="register"  width="800">

- Second time with the same user :

<img src="/pictures/register.png" title="register"  width="800">
```


## Login

- With right credentials :

<img src="/pictures/login.png" title="register"  width="800">

- With wrong credentials :

<img src="/pictures/login2.png" title="register"  width="800">
```


## Route protection

- With right credentials :

<img src="/pictures/route_protection.png" title="route protection"  width="800">

- With wrong credentials :

<img src="/pictures/route_protection2.png" title="route protection"  width="800">



## Refresh Token

- In **Package Manager Console** :
```
Add-Migration "Add refresh token"
Update-Database
```

### Tests

- Unexisting user :

<img src="/pictures/unexisting_user.png" title="unexisting user"  width="800">

- Existing user :

<img src="/pictures/existing_user.png" title="existing user"  width="800">

- Refresh token :

<img src="/pictures/refresh_token.png" title="refresh token"  width="800">

- Not yet expired token :

<img src="/pictures/token_not_yet_expired.png" title="token not yet expired"  width="800">
