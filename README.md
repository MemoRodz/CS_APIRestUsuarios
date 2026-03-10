# API Rest de Gestión de Usuarios 🚀

Sistema robusto de gestión de usuarios desarrollado con **.NET 10** y **C#**, siguiendo una arquitectura de capas (API, BLL, DAL) y principios de Clean Code.

## 📖 Descripción del proyecto
Este repositorio contiene una API REST.  
El objetivo del proyecto es ofrecer una estructura limpia, escalable y mantenible que pueda extenderse para la automatización de procesos administrativos y de negocio.

## 🛠 Tecnologías y Herramientas
- **Framework:** ASP.NET Core Web API (.NET 10)
- **Seguridad:** Encriptación AES-256 con derivación de llaves SHA-256.
- **Documentación:** Swagger / OpenAPI.
- **Pruebas:** xUnit, Moq y FluentAssertions.
- **Arquitectura:** Patrón Repositorio e Inyección de Dependencias.

## 📂 Estructura del Proyecto
- **Users.API:** Controladores, filtros globales de excepciones y configuración de middleware.
- **Users.BLL (Business Logic Layer):** Lógica de negocio, encriptación y validaciones (RFC, Teléfono).
- **Users.DAL (Data Access Layer):** Modelos de datos y repositorio en memoria (In-Memory).

## 🚀 Instalación y Uso
1. Clonar el repositorio.
2. Configurar la `Semilla` de encriptación en `appsettings.json`.
3. Ejecutar `dotnet run --project Users.API`.
4. Acceder a `https://localhost:7124/swagger` para probar los endpoints.

## 👨‍💻 Sobre mí
**Full Stack Developer** con más de 3 años de experiencia como Jefe de Unidad Departamental de Programación y Sistemas. Especializado en la gestión de equipos y desarrollo de soluciones con ASP Clásico. Con conocimientos sólidos en desarrollo Desktop y Web bajo el ecosistema de Visual Studio. Enfocado en resultados y coordinación de equipos de desarrollo.
<br/>  
*Nivel de inglés: Intermedio.*

## 📄 Licencia
   Este proyecto y su código está bajo la licencia MIT. Para producción se deberá apegarse a las condiciones de uso de **Microsoft** y licencias adjuntas.

## 🤝 Contribuciones
   ¡Las contribuciones son bienvenidas! Por favor, haz un fork del repositorio y envía un pull request con tus mejoras.
<br/>  
<div align="center">
            <a href="https://www.paypal.com/donate/?hosted_button_id=U39DC8PFHVMXY" target="_blank" style="display: inline-block;">
                <img
                    src="https://img.shields.io/badge/Donate-PayPal-blue.svg?style=flat-square&logo=paypal" 
                    align="center"
                />
            </a></div>
<br />   

   
   
