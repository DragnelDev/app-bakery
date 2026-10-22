namespace WebPasteleria.Helpers
{
    public static class EstadoPedidoHelper
    {
        public enum EstadoPedido
        {
            Pendiente = 0,
            Confirmado = 1,
            EnPreparacion = 2,
            ListoParaEntrega = 3,
            EnCamino = 4,
            Entregado = 5,
            Cancelado = 6
        }

        public static Dictionary<int, string> ObtenerNombresEstados()
        {
            return new Dictionary<int, string>
            {
                { 0, "Pendiente" },
                { 1, "Confirmado" },
                { 2, "En Preparación" },
                { 3, "Listo para Entrega" },
                { 4, "En Camino" },
                { 5, "Entregado" },
                { 6, "Cancelado" }
            };
        }

        public static Dictionary<int, string> ObtenerIconosEstados()
        {
            return new Dictionary<int, string>
            {
                { 0, "fa-clock-o" },
                { 1, "fa-check" },
                { 2, "fa-cutlery" },
                { 3, "fa-gift" },
                { 4, "fa-truck" },
                { 5, "fa-smile-o" },
                { 6, "fa-times" }
            };
        }

        public static Dictionary<int, string> ObtenerColoresEstados()
        {
            return new Dictionary<int, string>
            {
                { 0, "#ffc107" },  // Amarillo - Pendiente
                { 1, "#17a2b8" },  // Azul claro - Confirmado
                { 2, "#ff91c7" },  // Rosa - En Preparación
                { 3, "#28a745" },  // Verde - Listo
                { 4, "#007bff" },  // Azul - En Camino
                { 5, "#28a745" },  // Verde - Entregado
                { 6, "#dc3545" }  // Rojo - Cancelado
            };
        }

        public static string ObtenerNombreEstado(int estadoId)
        {
            var estados = ObtenerNombresEstados();
            return estados.ContainsKey(estadoId) ? estados[estadoId] : "Desconocido";
        }

        public static string ObtenerIconoEstado(int estadoId)
        {
            var iconos = ObtenerIconosEstados();
            return iconos.ContainsKey(estadoId) ? iconos[estadoId] : "fa-circle";
        }

        public static string ObtenerColorEstado(int estadoId)
        {
            var colores = ObtenerColoresEstados();
            return colores.ContainsKey(estadoId) ? colores[estadoId] : "#6c757d";
        }

        public static string ObtenerDescripcionEstado(int estadoId)
        {
            return estadoId switch
            {
                0 => "Tu pedido ha sido recibido y está pendiente de confirmación",
                1 => "Tu pedido ha sido confirmado y pronto comenzaremos a prepararlo",
                2 => "Estamos preparando tu pedido con los mejores ingredientes",
                3 => "Tu pedido está listo y puedes pasar a recogerlo o está listo para ser entregado",
                4 => "Tu pedido está en camino hacia tu dirección",
                5 => "Tu pedido ha sido entregado exitosamente. ¡Disfrútalo!",
                6 => "Tu pedido ha sido cancelado",
                _ => "Estado desconocido"
            };
        }

        public static int CalcularProgreso(int estadoId)
        {
            return estadoId switch
            {
                0 => 10,   // Pendiente
                1 => 30,   // Confirmado
                2 => 50,   // En Preparación
                3 => 70,   // Listo
                4 => 90,   // En Camino
                5 => 100,  // Entregado
                6 => 100, // Cancelado
                _ => 0
            };
        }

        public static bool PuedeActualizarA(int estadoActual, int nuevoEstado)
        {
            // Lógica de transiciones válidas
            if (estadoActual == -1) // Cancelado no puede cambiar
                return false;

            if (nuevoEstado == -1) // Siempre se puede cancelar
                return true;

            // Solo se puede avanzar al siguiente estado o retroceder uno
            return nuevoEstado == estadoActual + 1 || nuevoEstado == estadoActual - 1;
        }

        public static List<int> ObtenerEstadosSiguientes(int estadoActual)
        {
            var siguientes = new List<int>();

            if (estadoActual == -1 || estadoActual == 5)
                return siguientes; // Cancelado o Entregado no tienen siguientes

            // Agregar el siguiente estado
            if (estadoActual < 5)
            {
                siguientes.Add(estadoActual + 1);
            }

            // Siempre se puede cancelar (excepto si ya está entregado)
            if (estadoActual < 5)
            {
                siguientes.Add(-1);
            }

            return siguientes;
        }
    }
}