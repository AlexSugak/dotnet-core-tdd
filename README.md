# .NET outside-in tdd  
A sample project used to demonstrate TDD (the "outside-in" school) techniques on .NET Core platform.

## Why?
We all "know" that unit testing is good. Still, it is often the case that .NET projects, especially the ["zoo software"](http://blog.ploeh.dk/2012/12/18/RangersandZookeepers/) ones, lack proper test coverage. This project aims to provide a working example of how to do testing of a "real-world" ASP.NET application, hoping that it will make it easier for teams to adopt TDD practice.

## What?
The sample project is a stateless ASP.NET web api that writes and reads data to and from [MySQL](https://www.mysql.com/) database. The tests part of the project demonstrate how to develop the web api starting with "walking sekeleton", doing the "spike" and then driving refactoring using unit tests. 

## Prerequisites 
The project is tested on Mac with .NET Core version `2.1.403` installed, but it should work on Windows too. You should also have [docker](https://www.docker.com/get-started) installed in order to run integration tests. 

This project also uses [Makefile](https://www.gnu.org/software/make/manual/html_node/Introduction.html) to automate repetitive tasks. It should run on Mac and Linux out of the box, here is how to use [Makefiles on Windows](https://stackoverflow.com/a/17158442/4910910).  

## Building and running tests
You can run tests both from docker container and locally.

To run it from docker, do the following:
```bash
$ docker-compose up -d
$ make log-watch
```
This will start both db and test app docker containers and run test log watcher. Change some files in the project and see how tests execute.

To run tests locally:
```bash
$ docker-compose up -d mysql
$ make test-watch
```
This will start mysql db docker container and run test watcher locally. Change some files in the project and see how tests execute.
