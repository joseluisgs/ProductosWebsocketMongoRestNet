# ProductosWebsocketMongoRestNet

Ejemplo de una API REST básica en .NET Core 8 con MongoDB y Websocket

- [ProductosWebsocketMongoRestNet](#productoswebsocketmongorestnet)
  - [Descripción](#descripción)
  - [Endpoints](#endpoints)
  - [Librerías usadas](#librerías-usadas)


## Descripción

Este proyecto es un ejemplo de una API REST básica en .NET Core 8 con MongoDB con Websocket para crear un sistema de notificaciones.

Es una ampliación de este [proyecto](https://github.com/joseluisgs/ProductosStorageMongoRestNet)

Cuidado con las configuraciones y la inyección de los servicios

Mongo esta en Mongo Atlas, por lo que la cadena de conexión es un poco diferente.

## Endpoints
- Books: contiene el CRUD de los libros (GET, POST, PUT, DELETE)
- ws: contiene el websocket para las notificaciones

## Librerías usadas
- MongoDB.Driver