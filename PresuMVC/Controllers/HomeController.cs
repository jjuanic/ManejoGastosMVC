using Microsoft.AspNetCore.Mvc;
using PresuMVC.Models;
using PresuMVC.Services;
using PresuMVC.Models;
using System.Diagnostics;

namespace PresuMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioIngresos repositorioIngresos;
        private readonly IRepositorioEgresos repositorioEgresos;

        public HomeController(ILogger<HomeController> logger, IRepositorioIngresos repositorioIngresos, IRepositorioEgresos repositorioEgresos)
        {

            _logger = logger;
            this.repositorioIngresos = repositorioIngresos;
            this.repositorioEgresos = repositorioEgresos;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fecha = null)
        {
            var fechaIndex = fecha ?? DateTime.Now;
            var ingresos = await repositorioIngresos.GetIngresosTotales(fechaIndex);
            var egresos = await repositorioEgresos.GetEgresosTotales(fechaIndex);

            var modelo = new IndexViewModel()
            {
                Fecha = new DateTime(fechaIndex.Year, fechaIndex.Month, 1),
                Ingresos = ingresos,
                Egresos = egresos
            };
            return View(modelo);
        }

        // Acción para cambiar de mes al siguiente
        [HttpPost]
        public IActionResult SiguienteMes(DateTime fecha)
        {
            var siguienteMes = fecha.AddMonths(1);
            return RedirectToAction("Index", new { fecha = siguienteMes });
        }

        [HttpPost]
        public IActionResult MesAnterior(DateTime fecha)
        {
            var mesAnterior = fecha.AddMonths(-1);
            return RedirectToAction("Index", new { fecha = mesAnterior });
        }

        [HttpGet]
        public IActionResult CreateIngreso(DateTime fechaIndex)
        {
            var modelo = new Ingreso
            {
                FechaRegistro = fechaIndex
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIngreso(Ingreso ingreso)
        {

            await repositorioIngresos.CreateIngreso(ingreso);

            return RedirectToAction("Index", new { fecha = ingreso.FechaRegistro.ToString("yyyy-MM") });
        }


        [HttpGet]
        public async Task<IActionResult> DeleteIngreso(int id, DateTime fechaIndex)
        {
            var ingreso = await repositorioIngresos.GetIngresoByID(id);
            var modelo = new IngresoUpdateDeleteViewModel{
                        IdIngreso = ingreso.IdIngreso,
                        Nombre = ingreso.Nombre,
                        Valor = ingreso.Valor,
                        FechaRegistro = ingreso.FechaRegistro,
                        FechaFin = ingreso.FechaFin,
                        IdTipoIngreso = ingreso.IdTipoIngreso,
                        FechaIndex = fechaIndex
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIngresoDef(int idIngreso, DateTime fechaIndex)
        {
            var ingreso = await repositorioIngresos.GetIngresoByID(idIngreso);

            if (ingreso.IdTipoIngreso == 2) // Único
            {
                await repositorioIngresos.DeleteIngreso(idIngreso);
                return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
            }

            if (ingreso.IdTipoIngreso == 1)
            {
                ingreso.FechaFin = fechaIndex.AddMonths(-1);
                if (ingreso.FechaFin.Value < ingreso.FechaRegistro)
                {
                    await repositorioIngresos.DeleteIngreso(ingreso.IdIngreso);
                    return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
                }
                else
                {
                    await repositorioIngresos.UpdateIngreso(ingreso);
                    return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
                }
            }
            //}

            return View(ingreso);
        }

        [HttpGet]
        public async Task<IActionResult> EditIngreso(int id, DateTime fechaIndex)
        {
            var ingreso = await repositorioIngresos.GetIngresoByID(id);
            var modelo = new IngresoUpdateDeleteViewModel
            {
                IdIngreso = ingreso.IdIngreso,
                Nombre = ingreso.Nombre,
                Valor = ingreso.Valor,
                FechaRegistro = ingreso.FechaRegistro,
                FechaFin = ingreso.FechaFin,
                IdTipoIngreso = ingreso.IdTipoIngreso,
                FechaIndex = fechaIndex
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateIngreso(IngresoUpdateDeleteViewModel ingresoViewModel)
        {
            var ingresoModificado = new Ingreso // Ingreso Modificado
            {
                IdIngreso = ingresoViewModel.IdIngreso,
                Nombre = ingresoViewModel.Nombre,
                Valor = ingresoViewModel.Valor,
                FechaRegistro = ingresoViewModel.FechaRegistro,
                FechaFin = ingresoViewModel.FechaFin,
                IdTipoIngreso = ingresoViewModel.IdTipoIngreso,
            };

            var fechaIndex = ingresoViewModel.FechaIndex;

            if (ingresoModificado.IdTipoIngreso == 2) // Único
            {
                await repositorioIngresos.UpdateIngreso(ingresoModificado);

            } else // Mensual, dividimos el ingreso en 2.
            {
                // Traemos el ingreso sin modificar

                var ingresoViejo = await repositorioIngresos.GetIngresoByID(ingresoModificado.IdIngreso);

                // A este ingreso viejo, le asignamos una fecha de fin = index - 1

                if (ingresoViejo.FechaFin.HasValue && (ingresoViejo.FechaFin.Value.Month == fechaIndex.Month))
                {
                    await repositorioIngresos.UpdateIngreso(ingresoModificado);
                } else { 

                // Ahora, creamos un ingreso nuevo, para esto, asignamos al ingreso modificado id = 0
                // y una fecha de creación igual al index.
                ingresoModificado.IdIngreso = 0;
                ingresoModificado.FechaRegistro = fechaIndex;
                ingresoModificado.FechaFin = ingresoViejo.FechaFin;

                ingresoViejo.FechaFin = fechaIndex.AddMonths(-1);

                await repositorioIngresos.CreateIngreso(ingresoModificado);

                // Modificamos el ingreso viejo para ponerle una fecha de fin.
                await repositorioIngresos.UpdateIngreso(ingresoViejo);
                }
            }            

            return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
        }

        [HttpGet]
        public IActionResult CreateEgreso(DateTime fechaIndex)
        {
            var modelo = new Egreso
            {
                FechaRegistro = fechaIndex
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEgreso(Egreso egreso)
        {
            if (egreso.IdTipoEgreso == 2 || egreso.IdTipoEgreso == 1)
            {
                await repositorioEgresos.CreateEgreso(egreso);
            }
            else
            {
                var cantCuotas = egreso.CantCuotas.Value;
                egreso.CuotaNro = 1;
                egreso.FechaCuota = egreso.FechaRegistro;
                egreso.FechaFin = egreso.FechaRegistro.AddMonths(cantCuotas);
                egreso.ValorTotal = egreso.Valor * egreso.CantCuotas;


                var idEgresoOriginal = await repositorioEgresos.CreateEgreso(egreso);

                
                for (var i=2; i<= cantCuotas; i++)
                {
                    var egresoCuota = new Egreso
                    {
                        Nombre = egreso.Nombre,
                        Valor = egreso.Valor,
                        FechaRegistro = egreso.FechaRegistro,
                        FechaFin = egreso.FechaFin,
                        CuotaNro = i,
                        CantCuotas = egreso.CantCuotas,
                        FechaCuota = egreso.FechaRegistro.AddMonths(i - 1),
                        ValorTotal = egreso.ValorTotal,
                        IdTipoEgreso = egreso.IdTipoEgreso,
                        IdEgresoOriginal= idEgresoOriginal
                    };
                    await repositorioEgresos.CreateEgreso(egresoCuota);
                }
            }
            return RedirectToAction("Index", new { fecha = egreso.FechaRegistro.ToString("yyyy-MM") });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEgreso(int id, DateTime fechaIndex)
        {
            var egreso = await repositorioEgresos.GetEgresoByID(id);
            var modelo = new EgresoUpdateDeleteViewModel
            {
                IdEgreso = egreso.IdEgreso,
                Nombre = egreso.Nombre,
                Valor = egreso.Valor,
                FechaRegistro = egreso.FechaRegistro,
                FechaFin = egreso.FechaFin,
                CuotaNro = egreso.CuotaNro,
                CantCuotas = egreso.CantCuotas,
                FechaCuota = egreso.FechaCuota,
                ValorTotal = egreso.ValorTotal,
                IdTipoEgreso = egreso.IdTipoEgreso,
                IdEgresoOriginal = egreso.IdEgresoOriginal,
                FechaIndex = fechaIndex
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEgreso(int idEgreso, DateTime fechaIndex, int tipoEliminacion)
        {
            var egreso = await repositorioEgresos.GetEgresoByID(idEgreso);

            if (egreso.IdTipoEgreso == 2) // Único
            {
                await repositorioEgresos.DeleteEgreso(idEgreso);
                return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
            }

            if (egreso.IdTipoEgreso == 1)
            {
                egreso.FechaFin = fechaIndex.AddMonths(-1);
                if (egreso.FechaFin.Value < egreso.FechaRegistro)
                {
                    await repositorioEgresos.DeleteEgreso(egreso.IdEgreso);
                    return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
                }
                else
                {
                    await repositorioEgresos.UpdateEgreso(egreso);
                    return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
                }
            }

            if (egreso.IdTipoEgreso == 3)
            {
                if (tipoEliminacion == 1) // Eliminar esta cuota en particular
                {
                    await repositorioEgresos.DeleteEgreso(idEgreso);
                } 
                
                if (tipoEliminacion == 2) // Eliminar esta cuota y las siguientes
                {
                    if (egreso.IdEgresoOriginal.HasValue) 
                    {
                    await repositorioEgresos.DeleteEgresoCuotasSiguientes(egreso.IdEgresoOriginal.Value, egreso.CuotaNro.Value);
                    } else
                    {
                        await repositorioEgresos.DeleteEgresoCuotasSiguientes(egreso.IdEgreso, egreso.CuotaNro.Value);
                    }
                }

                if (tipoEliminacion == 3) // Eliminar todas las cuotas (desde el inicio)
                {
                    if (egreso.IdEgresoOriginal.HasValue)
                    {
                        await repositorioEgresos.DeleteEgresoTodasCuotas(egreso.IdEgresoOriginal.Value);
                    }
                    else
                    {
                        await repositorioEgresos.DeleteEgresoTodasCuotas(egreso.IdEgreso);
                    }
                }
               
            }


            return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
        }

        [HttpGet]
        public async Task<IActionResult> EditEgreso(int id, DateTime fechaIndex)
        {
            var egreso = await repositorioEgresos.GetEgresoByID(id);
            var modelo = new EgresoUpdateDeleteViewModel
            {
                IdEgreso = egreso.IdEgreso,
                Nombre = egreso.Nombre,
                Valor = egreso.Valor,
                FechaRegistro = egreso.FechaRegistro,
                FechaFin = egreso.FechaFin,
                CuotaNro = egreso.CuotaNro,
                CantCuotas = egreso.CantCuotas,
                FechaCuota = egreso.FechaCuota,
                ValorTotal = egreso.ValorTotal,
                IdTipoEgreso = egreso.IdTipoEgreso,
                IdEgresoOriginal = egreso.IdEgresoOriginal,
                FechaIndex = fechaIndex
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> EditEgreso(EgresoUpdateDeleteViewModel egresoViewModel)
        {
            var egresoModificado = new Egreso
            {
                IdEgreso = egresoViewModel.IdEgreso,
                Nombre = egresoViewModel.Nombre,
                Valor = egresoViewModel.Valor,
                FechaRegistro = egresoViewModel.FechaRegistro,
                FechaFin = egresoViewModel.FechaFin,
                CuotaNro = egresoViewModel.CuotaNro,
                CantCuotas = egresoViewModel.CantCuotas,
                FechaCuota = egresoViewModel.FechaCuota,
                ValorTotal = egresoViewModel.ValorTotal,
                IdTipoEgreso = egresoViewModel.IdTipoEgreso,
                IdEgresoOriginal = egresoViewModel.IdEgresoOriginal
            };


            var fechaIndex = egresoViewModel.FechaIndex;

            if (egresoModificado.IdTipoEgreso == 2) // Único
            {
                await repositorioEgresos.UpdateEgreso(egresoModificado);

            }
            
            if (egresoModificado.IdTipoEgreso == 1)// Suscripción, dividimos el egreso en 2.
            {
                // Traemos el egreso sin modificar

                var egresoViejo = await repositorioEgresos.GetEgresoByID(egresoModificado.IdEgreso);

                if (egresoViejo.FechaFin.HasValue && (egresoViejo.FechaFin.Value.Month == fechaIndex.Month)) // Si estamos modificando en la fecha final
                {
                    await repositorioEgresos.UpdateEgreso(egresoModificado);
                }
                else
                {
                    // Ahora, creamos un egreso nuevo, para esto, asignamos al egreso modificado id = 0
                    // y una fecha de creación igual al index.
                    egresoModificado.IdEgreso = 0;
                    egresoModificado.FechaRegistro = fechaIndex;
                    egresoModificado.FechaFin = egresoViejo.FechaFin;

                    // A este egreso viejo, le asignamos una fecha de fin = index - 1
                    egresoViejo.FechaFin = fechaIndex.AddMonths(-1);

                    await repositorioEgresos.CreateEgreso(egresoModificado);

                    // Modificamos el egreso viejo para ponerle una fecha de fin.
                    await repositorioEgresos.UpdateEgreso(egresoViejo);
                }
            }

            if (egresoModificado.IdTipoEgreso == 3)
            {

            }
            

            return RedirectToAction("Index", new { fecha = fechaIndex.ToString("yyyy-MM") });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
