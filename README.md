# PRAKTYKA ZAWODOWA 2020

## Optical Graph Recognition App - Server

REST API that serves *Graph6* and *GraphML* files based on given picture containing graph.  It also integrates Google Accounts, so user is able to call chosen GoogleAPIs.

To see more about image processing itself, visit [script's repository](https://github.com/Praktyka-Zawodowa-2020/optical_graph_recognition).

## Deployment with Docker-Compose 

### Locally

Visit [this page](https://docs.docker.com/docker-for-windows/install/) in order to install *Docker for Windows*.

To deploy this server locally, use compose commands listed below - execute them in a **Api** folder with **.yml** files in it:

- firstly, to build service(s):

`$ docker-compose build`

- secondly, to deploy:

`$ docker-compose -f docker-compose.yml -f docker-compose.prod.yml up`

Then, you can access *https://localhost/swagger* to see API specification.

In order to TLS work properly, a CA Certificate needs to be provided for Kestrel (.netCore http server), e.g. Self-Signed Certificate. Check **Api/Api/Certs/howTo.txt** for further instructions on how to generate such one. 

Then, Client App must trust this certificate; for Windows/Chrome **double click .pfx* -> *Install certificate* and insert it in *Trusted Root Certification Authorities storage*. For more instructions how to trust certificates on certain OSs, check [this page](http://wiki.cacert.org/FAQ/ImportRootCert).

### Remotely

Same procedure as locally. However, if the remote server has own domain, [Let's Encrypt](https://letsencrypt.org/) certificate can be provided for Kestrel and therefore Client App won't need to trust any cert by itself, as they are trusted by many OS by default. To do so, another .yml file need to be created, e.g. **docker-compose.ogr.yml** that specifies: 
* volume to the cert (*when container starts it will create an empty directory for host, where cert needs to be put, but throw an error because file doesn't exist yet; just put them there and restart container; file can be also provided before creating container, but be careful about paths*)
* *ASPNETCORE_ENVIRONMENT* variable, which points at certain appsetttings.json file (e.g. **appsettings.OgrServer.json**), which specifies an actual path for **.pfx** file along with its password (previously created volume points at folder with this files inside the container). 

Having those, just deploy server with command:

`$ docker-compose -f docker-compose.yml -f docker-compose.ogr.yml up`

## Database

For now, this service uses *SQLite* engine, which serves as a simple, self-contained, one-file database. Container will create db file by itself. For science purposes, volume mount is provided - file will be located at **shared/Data** folder in the root folder of cloned repository.

## Google Accounts & Authentication 

This API is designed to use [Google Implicit Flow](https://developers.google.com/identity/protocols/oauth2/javascript-implicit-flow) in order to create and authenticate users. Although, for authorization, regular JWTokens are implemented by API itself.

### Google Developers Project

In order to make this app work properly, you need to obtain application credentails and create consent screen at Google's Developer Console. They contains of *ClientId*, *ClientSecret* and *RedirectURL*. Those need to be defined in **appsettings.json** file. 

### API Authentication & Authorization

Client Side Application needs to call provided Google endpoint (depends on technology used for client app) requesting two tokens - *IdToken*, which contains Google account details, such as email, name, claims etc.; along with *AuthCode*, which allows API to obtain Google credentails used to call and authorize proper GoogleApis i.e. DriveApi. 

Those credentials consist of *access_token* and *refresh_token*,  that will be stored in database, so API can call GoogleApis on user's behalf forever (until the *refresh_token* expires or is revoked, which requires again authentication). **IMPORTANT - refresh token is acquired only the first time user accepts consent screen, so if ever Google credentails are lost from database - user must remove application in account's settings and authenticate API again.**. 

If Client Side Application succesfully obtains *IdToken* and *AuthCode* (remember, that APIs *ClientId* must be in the clients request to the Google endpoint, so that API will know, that client wants to authenticate our app specifically), those can be requested at this API to authenticate user by Google account. API verifies if *IdToken* is meant for our API and if so - extracts information about user and creates user entity in database. Then if successful, requests *access_token* and *request_token* for authorizing GoogleApis. 

If everything goes well - authorization is taken over by API. *JWToken* is returned along with *RefreshToken*. Until valid, *JWToken* is used to authorize endpoints by adding additional header "Authorization: Bearer *JWToken*". If *JWToken* expires, *RefreshToken* can be used to obtain new *JWToken* with new *RefreshToken*. It eliminates the need of authenticating user every time he calls API - to do so, *RefreshToken* needs to be stored in clients storage. If *RefreshToken* expires - there's no other option - user must auhtenticate again. 

#### Grzegorz Choiński 2020