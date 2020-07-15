# PRAKTYKA ZAWODOWA 2020

## Optical Graph Recognition App - Server

REST API that serves *Graph6* and *GraphML* files based on given picture containing graph. It also integrates GoogleDriveApi, so files can be send directly to user's Drive.

## Deploying with Docker-Compose

Visit [this page](https://docs.docker.com/docker-for-windows/install/) in order to install *Docker for Windows*.

To deploy this server locally, use compose commands listed below - execute them in a **Api** folder with **.yml** files in it:

- firstly, to build service(s):

`$ docker-compose build`

- secondly, to deploy:

`$ docker-compose -f docker-compose.yml -f docker-compose.prod.yml up`

Then, you can access *https://localhost/swagger* to see API specification.

## Database

For now, this service uses *SQLite* engine, which serves as a simple, self-contained, one-file database. Container will create db file by itself. For science purposes, volume mount is provided - file will be located at **Data** folder in the root folder of cloned repository.


#### Grzegorz Choiński 2020