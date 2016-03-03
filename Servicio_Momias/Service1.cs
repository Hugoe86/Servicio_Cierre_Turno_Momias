using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using Operaciones.Turnos.Negocio;
using Erp.Constantes;
using Erp.Seguridad;
using Erp_Apl_Parametros.Negocio;
using MySql.Data.MySqlClient;
using Erp_Solicitud_Facturacion.Negocio;
using Erp.Ayudante_Sintaxis;
using Erp.Helper;

namespace Servicio_Momias
{
    public partial class Service1 : ServiceBase
    {
        #region Variables
        public Timer Tiempo;
        #endregion

        //*************************************************************************************
        //NOMBRE DE LA FUNCIÓN: Service1
        //DESCRIPCIÓN: inicia Session el usuario que se logueo
        //PARÁMETROS :
        //CREO       : Hugo Enrique Ramírez Aguilera
        //FECHA_CREO : 16-Julio-2015
        //MODIFICO:
        //FECHA_MODIFICO
        //CAUSA_MODIFICACIÓN
        //*************************************************************************************
        public Service1()
        {
            InitializeComponent();
            Tiempo = new Timer();
            Tiempo.Interval = 3600;// se ejecutara cada hora // 36000000
            Tiempo.Elapsed += new ElapsedEventHandler(Tiempo_Elapsed);
        }
        //*************************************************************************************
        //NOMBRE DE LA FUNCIÓN: Iniciar_Sesion_Usuario
        //DESCRIPCIÓN: inicia Session el usuario que se logueo
        //PARÁMETROS :
        //CREO       : Hugo Enrique Ramírez Aguilera
        //FECHA_CREO : 16-Julio-2015
        //MODIFICO:
        //FECHA_MODIFICO
        //CAUSA_MODIFICACIÓN
        //*************************************************************************************
        protected override void OnStart(string[] args)
        {
            Tiempo.Enabled = true;
        }
        //*************************************************************************************
        //NOMBRE DE LA FUNCIÓN: Iniciar_Sesion_Usuario
        //DESCRIPCIÓN: 
        //PARÁMETROS :
        //CREO       : Hugo Enrique Ramírez Aguilera
        //FECHA_CREO : 16-Julio-2015
        //MODIFICO:
        //FECHA_MODIFICO
        //CAUSA_MODIFICACIÓN
        //*************************************************************************************
        protected override void OnStop()
        {
        }

        #region Metodos_Generales

        //*************************************************************************************
        //NOMBRE DE LA FUNCIÓN: Tiempo_Ejecucion
        //DESCRIPCIÓN: 
        //PARÁMETROS :
        //CREO       : Hugo Enrique Ramírez Aguilera
        //FECHA_CREO : 16-Julio-2015
        //MODIFICO:
        //FECHA_MODIFICO
        //CAUSA_MODIFICACIÓN
        //*************************************************************************************
        public void Tiempo_Elapsed(object sender, EventArgs e)
        {
            Cls_Ope_Turnos_Negocio Rs_Turno = new Cls_Ope_Turnos_Negocio();
            DataTable Dt_Turno = new DataTable();
            DateTime Dtime_Fecha_Turno = new DateTime();
            try
            {
                //  validacion para que el turno se cerro anteriormente *************************************
                Rs_Turno.P_Estatus = "ABIERTO";
                Dt_Turno = Rs_Turno.Consultar_Turnos();

                //  validamos que contenga algun turno abierto
                if (Dt_Turno != null && Dt_Turno.Rows.Count > 0)
                {
                    foreach (DataRow Registro in Dt_Turno.Rows)
                    {
                        Dtime_Fecha_Turno = Convert.ToDateTime(Registro["Hora_Inicio"].ToString());
                    }
                }

                //  comparamos la fecha actual contra la fecha del turno
                if (Dtime_Fecha_Turno.ToString("dd/MM/yyyy") != DateTime.Now.ToString("dd/MM/yyyy"))
                {
                    //  si es distinta se procedara a cerrar el turno
                    Rs_Turno.P_Fecha_Hora_Cierre = DateTime.Now;
                    Rs_Turno.Cierre_Turno_Fuera_Fecha();

                    //if (Validar_Conexion())
                    //{
                    //    Exportar_Informacion();
                    //}
                    //else
                    //{
                    //}
                }

                //  validamos el cambio de año
                if (Convert.ToInt32(DateTime.Now.ToString("dd")) == 15 && Convert.ToInt32(DateTime.Now.ToString("MM")) == 12)
                {
                    //  se cambia el estatus de los productos
                    Rs_Turno.Cambio_Producto_X_Anio();
                }

            }
            catch (Exception Ex)
            {
                throw new Exception("Iniciar_Sesion_Usuario: " + Ex.Message);
            }

        }

        ///*******************************************************************************************************
        ///NOMBRE_FUNCIÓN: Validar_Conexion
        ///DESCRIPCIÓN: valida que todos los campos obligatorios hayan sido llenados por el usuario
        ///PARÁMETROS:
        ///CREO: Roberto González Oseguera
        ///FECHA_CREO: 07-oct-2013
        ///MODIFICÓ: 
        ///FECHA_MODIFICÓ: 
        ///CAUSA_MODIFICACIÓN: 
        ///*******************************************************************************************************
        private Boolean Validar_Conexion()
        {
            Boolean Estatus_Conexion = false;
            Cls_Apl_Parametros_Negocio Consulta_Parametros = new Cls_Apl_Parametros_Negocio();
            DataTable Dt_Parametros = new DataTable();
            String StrConexion = "";
            MySqlConnection Obj_Conexion = null;

            try
            {
                Consulta_Parametros.P_Parametro_Id = "00001";
                Dt_Parametros = Consulta_Parametros.Consultar_Parametros();

                foreach (DataRow Registro in Dt_Parametros.Rows)
                {
                    StrConexion = "Server=" + Registro[Cat_Parametros.Campo_Ip_A_Enviar_Ventas].ToString() + ";";
                    StrConexion += "Database=" + Registro[Cat_Parametros.Campo_Base_Datos_A_Enviar_Ventas].ToString() + ";";
                    StrConexion += "Uid=" + Registro[Cat_Parametros.Campo_Usuario_A_Enviar_Ventas].ToString() + ";";
                    StrConexion += "Pwd=" + Cls_Seguridad.Desencriptar(Registro[Cat_Parametros.Campo_Contrasenia_A_Enviar_Ventas].ToString()) + ";";
                }

                try
                {
                    Obj_Conexion = new MySqlConnection(StrConexion);
                    Obj_Conexion.Open();
                    Obj_Conexion.Close();
                    Estatus_Conexion = true;
                }
                catch (Exception es)
                {
                    Estatus_Conexion = false;
                }
            }
            catch (Exception E)
            {
            }

            return Estatus_Conexion;
        }

        ///*******************************************************************************************************
        ///NOMBRE_FUNCIÓN: Btn_Exportar_Click
        ///DESCRIPCIÓN: Exporta la informacion de las ventas
        ///PARÁMETROS:
        ///CREO: Hugo Enrique Ramírez Aguilera
        ///FECHA_CREO: 23-Marzo-2015
        ///MODIFICÓ: 
        ///FECHA_MODIFICÓ: 
        ///CAUSA_MODIFICACIÓN: 
        ///*******************************************************************************************************
        private void Exportar_Informacion()
        {
            Cls_Apl_Parametros_Negocio Consulta_Parametros = new Cls_Apl_Parametros_Negocio();
            Cls_Ope_Solicitud_Facturacion_Negocio Obj_Enviar_Ventas_Dia = new Cls_Ope_Solicitud_Facturacion_Negocio();
            DataTable Dt_Parametros = new DataTable();
            DataTable Dt_Ventas_Dia = new DataTable();
            DataTable Dt_Cambios_Padron = new DataTable();
            DataTable Dt_Nuevos_Usuarios_Padron = new DataTable();
            DataTable Dt_Nuevos_Usuarios_Lista = new DataTable();
            DataTable Dt_Pendientes = new DataTable();
            DateTime Fecha;
            try
            {
                Consulta_Parametros.P_Parametro_Id = "00001";
                Dt_Parametros = Consulta_Parametros.Consultar_Parametros();

                Obj_Enviar_Ventas_Dia = new Cls_Ope_Solicitud_Facturacion_Negocio();
                Dt_Pendientes = Obj_Enviar_Ventas_Dia.Consultar_Historico();

                foreach (DataRow Registro in Dt_Pendientes.Rows)
                {
                    DateTime.TryParse(Registro[Ope_Historico_Exportacion.Campo_Fecha].ToString(), out Fecha);
                    Obj_Enviar_Ventas_Dia.P_Fecha_Venta = Fecha.ToString("yyyy-MM-dd");
                    Dt_Ventas_Dia = Obj_Enviar_Ventas_Dia.Consultar_Tabla_Adeudos();

                    Dt_Cambios_Padron = Obj_Enviar_Ventas_Dia.Consultar_Cambios_Padron();
                    Dt_Nuevos_Usuarios_Padron = Obj_Enviar_Ventas_Dia.Consultar_Nuevos_Usuarios_Padron();
                    Dt_Nuevos_Usuarios_Lista = Obj_Enviar_Ventas_Dia.Consultar_Nuevos_Usuarios_Listadeudor();

                    //  se pasan los valores al turno 
                    Obj_Enviar_Ventas_Dia.P_Fecha_Venta = Fecha.ToString("yyyy-MM-dd");
                    Obj_Enviar_Ventas_Dia.P_Estatus = true;
                    Obj_Enviar_Ventas_Dia.P_Dt_Ventas_Dia = Dt_Ventas_Dia;
                    Obj_Enviar_Ventas_Dia.P_Dt_Parametros = Dt_Parametros;
                    Obj_Enviar_Ventas_Dia.P_Dt_Padron_Nuevos = Dt_Nuevos_Usuarios_Padron;
                    Obj_Enviar_Ventas_Dia.P_Dt_Padron_Actualizacion = Dt_Cambios_Padron;
                    Obj_Enviar_Ventas_Dia.P_Dt_Listdeudor_Nuevos = Dt_Nuevos_Usuarios_Lista;
                    Obj_Enviar_Ventas_Dia.Enviar_Ventas_Dia();

                    //  se actualiza el historico
                    Obj_Enviar_Ventas_Dia.P_No_Turno = "Proceo Exportacion System - "  + DateTime.Now.ToString();
                    Obj_Enviar_Ventas_Dia.Actualizar_Historico();
                }
            }
            catch (Exception E)
            {
               
            }
        }

        #endregion
    }
}
