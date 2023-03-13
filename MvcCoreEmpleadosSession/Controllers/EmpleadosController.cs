using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MvcCoreEmpleadosSession.Extensions;
using MvcCoreEmpleadosSession.Models;
using MvcCoreEmpleadosSession.Repositories;

namespace MvcCoreEmpleadosSession.Controllers
{
    public class EmpleadosController : Controller
    {
        private RepositoryEmpleados repo;
        private IMemoryCache memoryCache;

        public EmpleadosController(RepositoryEmpleados repo
            , IMemoryCache memoryCache)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
        }

        public IActionResult EmpleadosFavoritos()
        {
            return View();
        }

        public IActionResult EmpleadosSessionOK(int? idempleado
            , int? idfavorito)
        {
            if (idfavorito != null)
            {
                List<Empleado> empleadosFavoritos;
                if (this.memoryCache.Get("FAVORITOS") == null)
                {
                    empleadosFavoritos = new List<Empleado>();
                }
                else
                {
                    empleadosFavoritos = this.memoryCache.Get<List<Empleado>>("FAVORITOS");
                }
                //BUSCAMOS AL EMPLEADO EN BBDD PARA ALMACENARLO EN CACHE
                Empleado empleado = this.repo.FindEmpleado(idfavorito.Value);
                empleadosFavoritos.Add(empleado);
                //ALMACENAMOS LOS DATOS EN CACHE
                this.memoryCache.Set("FAVORITOS", empleadosFavoritos);
            }


            if (idempleado != null)
            {
                List<int> idsEmpleado;
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") == null)
                {
                    //CREAMOS LA LISTA PARA LOS IDS
                    idsEmpleado = new List<int>();
                }
                else
                {
                    //RECUPERAMOS LOS IDS ALMACENADOS PREVIAMENTE
                    idsEmpleado =
                        HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                idsEmpleado.Add(idempleado.Value);
                HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleado);
                ViewData["MENSAJE"] = "Empleados almacenados: "
                    + idsEmpleado.Count;
            }
            List<Empleado> empleados = this.repo.GetEmpleados();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenadosOK(int? ideliminar)
        {
            //RECUPERAMOS LOS DATOS DE SESSION
            List<int> idsEmpleados =
                HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados == null)
            {
                //NO HAY NADA EN SESSION
                ViewData["MENSAJE"] = "No existen empleados almacenados";
                //DEVOLVEMOS LA VISTA SIN MODEL
                return View();
            }
            else
            {
                if (ideliminar != null)
                {
                    //ELIMINAMOS EL ELEMENTO QUE NOS HAN SOLICITADO
                    idsEmpleados.Remove(ideliminar.Value);
                    if (idsEmpleados.Count == 0)
                    {
                        HttpContext.Session.Remove("IDSEMPLEADOS");
                    }
                    else
                    {
                        //DEBEMOS ACTUALIZAR DE NUEVO SESSION
                        HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                    }
                }
                List<Empleado> empleadosSession =
                    this.repo.GetEmpleadosSession(idsEmpleados);
                return View(empleadosSession);
            }
        }








        public IActionResult SessionEmpleados(int? idempleado)
        {
            if (idempleado != null)
            {
                //QUEREMOS ALMACENAR ALGO
                Empleado empleado = this.repo.FindEmpleado(idempleado.Value);
                //ALMACENAREMOS UNA COLECCION DE EMPLEADOS
                List<Empleado> empleadosSession;
                if (HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS") != null)
                {
                    //TENEMOS EMPLEADOS ALMACENADOS
                    empleadosSession =
                        HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS");
                }
                else
                {
                    //NO EXISTEN EMPLEADOS TODAVIA
                    empleadosSession = new List<Empleado>();
                }
                //AGREGAMOS EL NUEVO EMPLEADO A NUESTRA COLECCION
                empleadosSession.Add(empleado);
                //REFRESCAMOS LOS DATOS DE SESSION
                HttpContext.Session.SetObject("EMPLEADOS", empleadosSession);
                ViewData["MENSAJE"] = "Empleado " + empleado.Apellido
                    + " almacenado en Session.";
            }
            List<Empleado> empleados = this.repo.GetEmpleados();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenados()
        {
            return View();
        }

        public IActionResult SessionSalarios(int? salario)
        {
            if (salario != null)
            {
                int sumasalarial = 0;
                //PREGUNTAMOS SI YA TENEMOS DATOS ALMACENADOS EN SESSION
                if (HttpContext.Session.GetString("SUMASALARIAL") != null)
                {
                    //RECUPERAMOS LO QUE TENGAMOS ALMACENADO
                    sumasalarial =
                        int.Parse(HttpContext.Session.GetString("SUMASALARIAL"));
                }
                //SUMAMOS EL SALARIO RECIBIDO A NUESTRA VARIABLE 
                sumasalarial += salario.Value;
                //ALMACENAMOS EL NUEVO VALOR EN SESSION
                HttpContext.Session.SetString("SUMASALARIAL", sumasalarial.ToString());
                ViewData["MENSAJE"] = "Salario almacenado: " + salario.Value;
            }
            List<Empleado> empleados = this.repo.GetEmpleados();
            return View(empleados);
        }

        public IActionResult SumaSalarios()
        {
            return View();
        }
    }
}
