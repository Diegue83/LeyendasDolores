// Guion de los cuatro niveles: qué pasa al tocar cada zona, qué capas se ven según el progreso,
// qué objeto necesita cada lugar, la guía del botón Pista y las notas del diario.
// Los textos están en DatosJuego (Dialogos); aquí solo se usan sus claves.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leyendas
{
    public partial class Juego
    {
        bool B(string b) { return E.B(b); }
        void Poner(string b) { E.Poner(b); Guardar(); }
        bool Rec(string vista, string obj) { return B("rec_" + vista + "_obj_" + obj); }

        // ---------- Qué acertijos tiene cada nivel (para el resumen de puntos) ----------
        public static string[] AcertijosDe(int n)
        {
            switch (n)
            {
                case 0: return new[] { "candado", "deslizante", "anillos", "candelabro", "campanas" };
                case 1: return new[] { "nieves", "nonograma", "jarras", "cronologia", "criptex" };
                case 2: return new[] { "canal", "pinata", "balanza", "hanoi", "simbolos" };
                default: return new[] { "cesar", "libros", "caja", "ruta", "caso", "final" };
            }
        }

        // ---------- Visibilidad de capas ----------
        bool CapaVisible(string v, Capa c)
        {
            string n = c.nombre;
            if (c.esObjeto)
            {
                if (B("rec_" + v + "_" + n)) return false;
                switch (v + "/" + n)
                {
                    case "n1_sacristia/obj_llave_campanario": return B("ok_deslizante");
                    case "n1_nave/obj_partitura": return B("ok_anillos");
                    case "n1_campanario/obj_corona": return B("ok_campanas");
                    case "n3_pasillo/obj_cajita": return B("ok_pinata");
                    case "n4_sala/obj_rueda": return B("ab_cajon");
                }
                return true;
            }
            switch (v + "/" + n)
            {
                case "n1_atrio/sacristia_abierta": return B("ok_candado");
                case "n1_atrio/torre_abierta": return B("ab_torre");
                case "n1_atrio/torre_luz": return B("ok_candelabro");
                case "n1_atrio/puerta_abierta": return B("ab_principal");
                case "n1_atrio/campanas_a":
                case "n1_atrio/campanas_b": return B("ok_campanas");
                case "n1_sacristia/cuadro_abierto": return B("ok_deslizante");
                case "n1_nave/rayo": return B("ok_anillos");
                case "n1_torre/iluminada": return B("ok_candelabro");
                case "n2_plaza/kiosko_abierto": return B("ab_kiosko");
                case "n2_plaza/neveria_abierta": return B("ok_nieves");
                case "n2_kiosko/papel_resuelto": return B("ok_nonograma");
                case "n2_estatua/placa_resuelta":
                case "n2_estatua/compartimento_abierto": return B("ok_cronologia");
                case "n1_torre/escalera_luz":
                case "n1_torre/luz_calida": return B("ok_candelabro");
                case "n1_torre/penumbra": return !B("ok_candelabro");
                case "n2_estatua/criptex": return B("ok_cronologia") && !B("ok_criptex");
                case "n3_pasillo/bodega_abierta": return B("ab_bodega");
                case "n3_pasillo/pinata": return !B("ok_pinata");
                case "n3_pasillo/dulces": return B("ok_pinata");
                case "n3_talavera/canal_resuelto": return B("ok_canal");
                case "n3_bodega/ollas_resueltas": return B("ok_hanoi");
                case "n3_bodega/ollas_inicio": return !B("ok_hanoi");
                case "n4_sala/cajon_abierto": return B("ab_cajon");
                case "n4_sala/vitrina_restaurada": return B("ok_final");
                case "n4_biblioteca/libros_ordenados": return B("ok_libros");
                case "n4_biblioteca/caja_abierta": return B("ok_caja");
                case "n4_patio/cofre_abierto": return B("ok_ruta");
            }
            return !c.esParche; // los recortes se ven, los parches no
        }

        // ---------- Objeto que se usa en cada zona (para "usar" desde la mochila) ----------
        string Necesita(string v, string z)
        {
            switch (v + "/" + z)
            {
                case "n1_atrio/puerta_torre": return "llave_campanario";
                case "n1_nave/roseton": return "vara";
                case "n1_torre/candelabro": return "vela_encendida";
                case "n1_campanario/campanas": return "partitura";
                case "n2_plaza/kiosko": return "llave_kiosko";
                case "n3_pasillo/pinata": return "palo";
                case "n3_pasillo/bodega": return "llave_bodega";
                case "n4_patio/cofre": return "llave_cofre";
                case "n4_patio/mapa": return "llave_cofre";
            }
            return null;
        }

        // ---------- Al entrar a una vista ----------
        void AlEntrar(string v)
        {
            if (v == "n1_nave" && !B("ab_principal")) { Poner("ab_principal"); Despues(.6f, () => Decir("n1_principal_abierta")); }
            // El caso de la campana robada: el sacristán (sospechoso y llaves de la torre) y la primera evidencia.
            if (v == "n1_sacristia" && !B("vio_tomas")) { Poner("vio_tomas"); Despues(.6f, () => Decir("n1_tomas")); }
            if (v == "n1_campanario" && !B("ev_llave")) { Poner("ev_llave"); Despues(.6f, () => Decir("n1_evidencia_llave")); }
        }

        void AlRecoger(string obj) { }

        void AlCombinar(string resultado)
        {
            if (resultado == "carta_revelada") { Poner("leida_carta"); Poner("ev_zurdo"); Despues(.4f, () => { Decir("n4_carta_info"); Decir("n4_evidencia_zurdo"); }); }
        }

        void CombinacionEspecial(string accion)
        {
            if (accion == "cesar")
                AbrirAcertijo("cesar", () => { QuitarObjeto("rueda"); QuitarObjeto("carta_revelada"); Poner("ok_cesar"); Decir("n4_cesar_ok"); });
        }

        // Tocar dos veces un objeto de la mochila.
        void UsarSolo(string obj)
        {
            if (obj == "cajita")
                AbrirAcertijo("simbolos", () => { QuitarObjeto("cajita"); Poner("ok_simbolos"); Decir("n3_simbolos_ok"); ObtenerPieza("badajo", ranuras[0].rectTransform); });
            else if (obj == "carta_revelada") Decir("n4_carta_info");
        }

        void Entrar(string vista, float retraso = .7f) { Despues(retraso, () => { if (!modal.activeSelf) IrA(vista); }); }

        void Golpear(string capa, float grados = 14f, string sonido = "golpe", float tono = 1f)
        {
            var c = CapaDe(capa);
            if (c != null) { var v = c.GetComponent<Vaiven>(); if (v != null) v.Golpe(grados); }
            if (sonido != null) Sonido.Tocar(sonido, tono);
        }

        // ======================================================= Acciones por zona
        void Accion(string v, string z)
        {
            switch (v)
            {
                case "n1_atrio": Atrio(z); break;
                case "n1_sacristia": Sacristia(z); break;
                case "n1_nave": Nave(z); break;
                case "n1_torre": Torre(z); break;
                case "n1_campanario": Campanario(z); break;
                case "n2_plaza": Plaza(z); break;
                case "n2_kiosko": Kiosko(z); break;
                case "n2_neveria": Neveria(z); break;
                case "n2_estatua": Estatua(z); break;
                case "n3_pasillo": Pasillo(z); break;
                case "n3_talavera": Talavera(z); break;
                case "n3_bodega": Bodega(z); break;
                case "n4_sala": Sala(z); break;
                case "n4_biblioteca": Biblioteca(z); break;
                case "n4_patio": Patio(z); break;
            }
        }

        // ---------------- Nivel 1 · Parroquia ----------------
        void Atrio(string z)
        {
            switch (z)
            {
                case "placa": Poner("leida_placa"); Decir("n1_placa"); break;
                case "puerta_sacristia":
                    if (B("ok_candado")) { IrA("n1_sacristia"); break; }
                    Decir("n1_sacristia_cerrada");
                    AbrirAcertijo("candado", () => { Poner("ok_candado"); Refrescar(); Sonido.Tocar("puerta"); Decir("n1_candado_ok"); Entrar("n1_sacristia", 1.2f); });
                    break;
                case "puerta_torre":
                    if (B("ab_torre")) { IrA("n1_torre"); break; }
                    if (Lleva("llave_campanario")) { QuitarObjeto("llave_campanario"); Poner("ab_torre"); Refrescar(); Sonido.Tocar("puerta"); Decir("n1_torre_abre"); Entrar("n1_torre", 1.3f); }
                    else Decir("n1_torre_cerrada");
                    break;
                case "puerta_principal":
                    if (B("ab_principal")) IrA("n1_nave"); else Decir("n1_principal_cerrada");
                    break;
                case "roseton": Decir("n1_roseton_fuera"); break;
                case "campanas": if (B("ok_campanas")) Sonido.Tocar("campana_grande"); else Decir("n1_campanas_mudas"); break;
            }
        }

        void Sacristia(string z)
        {
            switch (z)
            {
                case "cuadro":
                    if (B("ok_deslizante")) break;
                    Decir("n1_cuadro");
                    AbrirAcertijo("deslizante", () => { Poner("ok_deslizante"); Refrescar(); Sonido.Tocar("puerta", .8f); Decir("n1_cuadro_ok"); });
                    break;
                case "ropero": Decir("n1_ropero"); break;
                case "ventana": Decir("n1_ventana"); break;
                case "puerta_nave": IrA("n1_nave"); break;
            }
        }

        void Nave(string z)
        {
            switch (z)
            {
                case "roseton":
                    if (B("ok_anillos")) { Decir("n1_roseton_ok"); break; }
                    if (!Lleva("vara")) { Decir("n1_roseton_alto"); break; }
                    AbrirAcertijo("anillos", () => { QuitarObjeto("vara"); Poner("ok_anillos"); Refrescar(); Sonido.Tocar("acierto", .8f); Decir("n1_roseton_ok"); });
                    break;
                case "altar": Decir("n1_altar"); break;
                case "retablo": Decir("n1_retablo"); break;
                case "candelero": Decir("n1_candelero"); break;
                case "puerta_sacristia": IrA("n1_sacristia"); break;
            }
        }

        void Torre(string z)
        {
            switch (z)
            {
                case "candelabro":
                    if (B("ok_candelabro")) break;
                    if (!Lleva("vela_encendida")) { Decir("n1_candelabro_oscuro"); break; }
                    AbrirAcertijo("candelabro", () => { QuitarObjeto("vela_encendida"); Poner("ok_candelabro"); Refrescar(); Decir("n1_candelabro_ok"); });
                    break;
                case "escalera":
                    if (B("ok_candelabro")) IrA("n1_campanario"); else Decir("n1_escalera_oscura");
                    break;
            }
        }

        void Campanario(string z)
        {
            if (z != "campanas") return;
            if (B("ok_campanas")) { Golpear("campana_grande", 10, "campana_grande"); return; }
            if (!Lleva("partitura"))
            {
                Golpear("campana_mediana", 8, "campana_mediana"); Decir("n1_campanas_sin");
                return;
            }
            AbrirAcertijo("campanas", () =>
            {
                QuitarObjeto("partitura"); Poner("ok_campanas"); Refrescar();
                Golpear("campana_grande", 16, "campana_grande");
                Despues(.35f, () => Golpear("campana_mediana", 16, "campana_mediana"));
                Despues(.7f, () => Golpear("campana_chica", 16, "campana_chica"));
                Decir("n1_campanas_ok");
            });
        }

        // ---------------- Nivel 2 · Plaza ----------------
        void Plaza(string z)
        {
            switch (z)
            {
                case "chuy":
                    if (B("ok_nieves")) { Decir("n2_chuy_despues"); break; }
                    Poner("vio_chuy");
                    Decir("n2_chuy_1");
                    AbrirAcertijo("nieves", () => { Poner("ok_nieves"); Refrescar(); DarObjeto("llave_kiosko", CapaDe("pj_chuy")?.imagen.rectTransform); Decir("n2_chuy_ok"); });
                    break;
                case "kiosko":
                    if (B("ab_kiosko")) { IrA("n2_kiosko"); break; }
                    if (Lleva("llave_kiosko")) { QuitarObjeto("llave_kiosko"); Poner("ab_kiosko"); Refrescar(); Sonido.Tocar("puerta"); Decir("n2_kiosko_abre"); Entrar("n2_kiosko", 1.3f); }
                    else Decir("n2_kiosko_cerrado");
                    break;
                case "neveria":
                    if (B("ok_nieves")) IrA("n2_neveria"); else Decir("n2_neveria_cerrada");
                    break;
                case "estatua": IrA("n2_estatua"); break;
            }
        }

        void Kiosko(string z)
        {
            switch (z)
            {
                case "papel_picado":
                    if (B("ok_nonograma")) break;
                    Decir("n2_papel");
                    AbrirAcertijo("nonograma", () => { Poner("ok_nonograma"); Refrescar(); DarObjeto("ficha4", vistaActual.Zona("papel_picado").transform as RectTransform); Decir("n2_papel_ok"); });
                    break;
                case "salida": Decir("n2_salida"); break;
            }
        }

        void Neveria(string z)
        {
            switch (z)
            {
                case "jarras":
                    if (B("ok_jarras")) break;
                    AbrirAcertijo("jarras", () => { Poner("ok_jarras"); DarObjeto("ficha5", vistaActual.Zona("jarras").transform as RectTransform); Decir("n2_jarras_ok"); });
                    break;
                case "garrafas": Decir("n2_garrafas"); break;
                case "letreros": Decir("n2_letreros"); break;
                case "puerta_plaza": IrA("n2_plaza"); break;
            }
        }

        void Estatua(string z)
        {
            switch (z)
            {
                case "placa":
                    if (B("ok_cronologia")) break;
                    Decir("n2_placa");
                    AbrirAcertijo("cronologia", () => { Poner("ok_cronologia"); Poner("ev_pie"); Refrescar(); Sonido.Tocar("puerta", .7f); Decir("n2_placa_ok"); });
                    break;
                case "compartimento":
                    if (!B("ok_cronologia")) { Decir("n2_compartimento"); break; }
                    if (B("ok_criptex")) break;
                    AbrirAcertijo("criptex", () =>
                    {
                        for (int i = 1; i <= 6; i++) QuitarObjeto("ficha" + i);
                        Poner("ok_criptex"); Refrescar(); Decir("n2_criptex_ok");
                        ObtenerPieza("yugo", vistaActual.Zona("compartimento").transform as RectTransform);
                    });
                    break;
            }
        }

        // ---------------- Nivel 3 · Mercado ----------------
        void Pasillo(string z)
        {
            switch (z)
            {
                case "lupita":
                case "balanza":
                    if (B("ok_balanza")) { Decir("n3_lupita_despues"); break; }
                    Poner("vio_lupita");
                    Decir("n3_lupita_1");
                    AbrirAcertijo("balanza", () => { Poner("ok_balanza"); DarObjeto("llave_bodega", CapaDe("pj_lupita")?.imagen.rectTransform); Decir("n3_lupita_ok"); });
                    break;
                case "pinata":
                    if (B("ok_pinata")) break;
                    if (!Lleva("palo")) { Golpear("pinata", 10, "golpe", 1.3f); Decir("n3_pinata_sin"); break; }
                    AbrirAcertijo("pinata", () =>
                    {
                        QuitarObjeto("palo"); Poner("ok_pinata"); Poner("ev_azul"); Refrescar();
                        Confeti.Lluvia(confeti, (Vector2)confeti.InverseTransformPoint(escenario.TransformPoint(escenario.rect.center)), 70);
                        Decir("n3_pinata_ok");
                    });
                    break;
                case "talavera": IrA("n3_talavera"); break;
                case "bodega":
                    if (B("ab_bodega")) { IrA("n3_bodega"); break; }
                    if (Lleva("llave_bodega")) { QuitarObjeto("llave_bodega"); Poner("ab_bodega"); Refrescar(); Sonido.Tocar("puerta"); Decir("n3_bodega_abre"); Entrar("n3_bodega", 1.3f); }
                    else Decir("n3_bodega_cerrada");
                    break;
            }
        }

        void Talavera(string z)
        {
            switch (z)
            {
                case "azulejos":
                    if (B("ok_canal")) break;
                    Decir("n3_alfarero_1");
                    AbrirAcertijo("canal", () => { Poner("ok_canal"); Refrescar(); Sonido.Tocar("agua"); DarObjeto("palo", vistaActual.Zona("alfarero").transform as RectTransform); Decir("n3_canal_ok"); });
                    break;
                case "alfarero": Decir(B("ok_canal") ? "n3_alfarero_despues" : "n3_alfarero_1"); break;
            }
        }

        void Bodega(string z)
        {
            switch (z)
            {
                case "ollas":
                    if (B("ok_hanoi")) break;
                    AbrirAcertijo("hanoi", () => { Poner("ok_hanoi"); Refrescar(); Decir("n3_ollas_ok"); });
                    break;
                case "costales": Decir("n3_costales"); break;
                case "puerta_pasillo": IrA("n3_pasillo"); break;
            }
        }

        // ---------------- Nivel 4 · Casa de Hidalgo ----------------
        void Sala(string z)
        {
            switch (z)
            {
                case "vitrina":
                    if (B("ok_final")) { Sonido.Tocar("campana_grande"); break; }
                    if (E.piezas.Count < 4) { Decir("n4_vitrina_falta"); break; }
                    // Primero se resuelve el caso (¿quién es El Campanero?), luego la confesión y al final se arma la campana.
                    if (!B("ok_caso")) { Decir("caso_antes"); AbrirAcertijo("caso", () => { Poner("ok_caso"); Decir("confesion", ArmarCampana); }); break; }
                    ArmarCampana();
                    break;
                case "escritorio":
                    if (!B("ab_cajon")) { Poner("ab_cajon"); Refrescar(); Sonido.Tocar("puerta", 1.4f, .7f); Decir("n4_cajon"); }
                    else if (B("rec_n4_sala_obj_rueda")) Decir("n4_cajon_vacio");
                    break;
                case "candelero": Decir("n4_candelero"); break;
                case "patio": IrA("n4_patio"); break;
                case "puerta_biblioteca": IrA("n4_biblioteca"); break;
            }
        }

        void Biblioteca(string z)
        {
            switch (z)
            {
                case "libros":
                    if (B("ok_libros")) { Decir("n4_libros_ok"); break; }
                    if (!B("ok_cesar")) { Decir("n4_libros_antes"); break; }
                    AbrirAcertijo("libros", () => { Poner("ok_libros"); Refrescar(); Decir("n4_libros_ok"); });
                    break;
                case "caja_fuerte":
                    if (B("ok_caja")) break;
                    AbrirAcertijo("caja", () => { Poner("ok_caja"); Refrescar(); Sonido.Tocar("puerta", .6f); DarObjeto("llave_cofre", vistaActual.Zona("caja_fuerte").transform as RectTransform); Decir("n4_caja_ok"); });
                    break;
                case "estantes": Decir("n4_estantes"); break;
                case "puerta_sala": IrA("n4_sala"); break;
            }
        }

        void Patio(string z)
        {
            switch (z)
            {
                case "mapa":
                case "cofre":
                    if (B("ok_ruta")) { Decir("n4_ruta_ok"); break; }
                    if (!Lleva("llave_cofre")) { Decir(z == "mapa" ? "n4_mapa" : "n4_cofre_cerrado"); break; }
                    Decir("n4_cofre_ruta");
                    AbrirAcertijo("ruta", () =>
                    {
                        QuitarObjeto("llave_cofre"); Poner("ok_ruta"); Refrescar(); Sonido.Tocar("puerta", .7f); Decir("n4_ruta_ok");
                        ObtenerPieza("cuerpo", vistaActual.Zona("cofre").transform as RectTransform);
                    });
                    break;
                case "morera": Decir("n4_morera"); break;
                case "puerta_sala": IrA("n4_sala"); break;
            }
        }

        // ======================================================= Guía del botón Pista (gratis)
        // Cada paso: bandera que lo completa, vista y zona donde se hace, y el consejo.
        static readonly string[][] GUIA =
        {
            new[] { "ok_candado", "n1_atrio", "puerta_sacristia", "La placa del atrio tiene una fecha en números romanos. El candado de la sacristía pide día y mes." },
            new[] { "rec_n1_sacristia_obj_vara", "n1_sacristia", "obj_vara", "En la sacristía hay una vara larga recargada en un rincón." },
            new[] { "rec_n1_sacristia_obj_vela", "n1_sacristia", "obj_vela", "Hay una vela sobre la mesa de la sacristía." },
            new[] { "ok_deslizante", "n1_sacristia", "cuadro", "El cuadro de la sacristía es un rompecabezas." },
            new[] { "rec_n1_sacristia_obj_llave_campanario", "n1_sacristia", "obj_llave_campanario", "Detrás del cuadro quedó algo brillando." },
            new[] { "ok_anillos", "n1_nave", "roseton", "Con la vara se pueden girar los anillos del rosetón de la nave." },
            new[] { "rec_n1_nave_obj_partitura", "n1_nave", "obj_partitura", "Recoge lo que iluminó el rayo de luz en el altar." },
            new[] { "rec_n1_nave_obj_cerillos", "n1_nave", "obj_cerillos", "Los cerillos del sacristán están al pie del candelero de la nave." },
            new[] { "combo_n1_vela_encendida", "", "", "En la mochila, toca la vela y luego los cerillos para encenderla." },
            new[] { "ab_torre", "n1_atrio", "puerta_torre", "La llave del campanario abre la puerta de la torre, en el atrio." },
            new[] { "ok_candelabro", "n1_torre", "candelabro", "Prende el candelabro de la torre con la vela encendida." },
            new[] { "ok_campanas", "n1_campanario", "campanas", "Sube al campanario y toca las campanas con la partitura." },
            new[] { "rec_n1_campanario_obj_corona", "n1_campanario", "obj_corona", "¡Recoge la corona!" },
            // Nivel 2
            new[] { "ok_nieves", "n2_plaza", "chuy", "Habla con Don Chuy, el del puesto de nieves." },
            new[] { "ab_kiosko", "n2_plaza", "kiosko", "La llave de Don Chuy abre la reja del kiosko." },
            new[] { "rec_n2_kiosko_obj_ficha1", "n2_kiosko", "obj_ficha1", "Hay una ficha de barro sobre la banca del kiosko." },
            new[] { "ok_nonograma", "n2_kiosko", "papel_picado", "El papel picado del kiosko se puede recortar." },
            new[] { "rec_n2_neveria_obj_ficha2", "n2_neveria", "obj_ficha2", "En el mostrador de la nevería hay una ficha." },
            new[] { "ok_jarras", "n2_neveria", "jarras", "Las jarras de agua de jamaica de la nevería esconden algo." },
            new[] { "rec_n2_plaza_obj_ficha3", "n2_plaza", "obj_ficha3", "Frente a los escalones del kiosko hay una ficha." },
            new[] { "rec_n2_plaza_obj_ficha6", "n2_plaza", "obj_ficha6", "Junto a la banca de herrería, frente al kiosko, hay una ficha." },
            new[] { "ok_cronologia", "n2_estatua", "placa", "La placa de la estatua pide ordenar lo que pasó en septiembre de 1810." },
            new[] { "ok_criptex", "n2_estatua", "compartimento", "Usa las seis fichas para abrir el criptex de la estatua." },
            // Nivel 3
            new[] { "ok_balanza", "n3_pasillo", "lupita", "Doña Lupita necesita encontrar la cocada hueca." },
            new[] { "ok_canal", "n3_talavera", "azulejos", "El alfarero presta su palo si arreglas el canal de azulejos." },
            new[] { "ok_pinata", "n3_pasillo", "pinata", "Con el palo puedes romper la piñata del pasillo." },
            new[] { "rec_n3_pasillo_obj_cajita", "n3_pasillo", "obj_cajita", "Entre los dulces de la piñata cayó una cajita." },
            new[] { "ab_bodega", "n3_pasillo", "bodega", "La llave de Doña Lupita abre la bodega del fondo." },
            new[] { "ok_hanoi", "n3_bodega", "ollas", "Las ollas de la bodega esconden la clave de la cajita." },
            new[] { "ok_simbolos", "", "", "Abre la cajita desde la mochila: tócala dos veces." },
            // Nivel 4
            new[] { "ab_cajon", "n4_sala", "escritorio", "El escritorio de la sala tiene un cajón." },
            new[] { "rec_n4_sala_obj_rueda", "n4_sala", "obj_rueda", "Toma la rueda de cifrado del cajón." },
            new[] { "rec_n4_sala_obj_carta", "n4_sala", "obj_carta", "Hay una carta en blanco sobre el escritorio." },
            new[] { "rec_n4_sala_obj_cerillos", "n4_sala", "obj_cerillos", "Los cerillos están junto al candelero vacío del escritorio." },
            new[] { "rec_n4_biblioteca_obj_vela", "n4_biblioteca", "obj_vela", "En la mesa de la biblioteca hay una vela." },
            new[] { "combo_n4_vela_encendida", "", "", "Enciende la vela con los cerillos (en la mochila)." },
            new[] { "combo_n4_carta_revelada", "", "", "Calienta la carta con la vela encendida: toca una y luego la otra." },
            new[] { "ok_cesar", "", "", "Combina la rueda de cifrado con la carta revelada." },
            new[] { "ok_libros", "n4_biblioteca", "libros", "Ordena los libros de la biblioteca por año." },
            new[] { "ok_caja", "n4_biblioteca", "caja_fuerte", "Los dígitos de los libros abren la caja fuerte: derecha, izquierda, derecha." },
            new[] { "ok_ruta", "n4_patio", "cofre", "Lleva la llave antigua al cofre bajo la morera, en el patio." },
            new[] { "ok_caso", "n4_sala", "vitrina", "Con las cuatro piezas, ve a la vitrina de la sala: es hora de descubrir quién es El Campanero." },
            new[] { "ok_final", "n4_sala", "vitrina", "¡Arma la campana en la vitrina de la sala!" },
        };

        void PistaGeneral()
        {
            if (modal.activeSelf || enTransicion) return;
            string pref = D.Nivel(E.nivel).id + "_";
            foreach (var g in GUIA)
            {
                bool delNivel = g[1].StartsWith(pref) || (g[1] == "" && g[0].Contains(pref.TrimEnd('_')))
                                || (g[1] == "" && g[0] == "ok_simbolos" && E.nivel == 2) || (g[1] == "" && g[0] == "ok_cesar" && E.nivel == 3);
                if (!delNivel || B(g[0])) continue;
                string donde = g[1] != "" && g[1] != VistaActual ? "  <i>(" + D.Vista(g[1]).nombre + ")</i>" : "";
                DecirTexto("pista", "", g[3] + donde);
                if (g[1] == VistaActual && vistaActual != null)
                {
                    var zn = vistaActual.Zona(g[2]);
                    if (zn != null) Chispa((RectTransform)zn.transform);
                }
                else if (g[1] != "" && bVolver.gameObject.activeSelf && D.Vista(VistaActual).padre != "") Chispa((RectTransform)bVolver.transform, 2.5f);
                return;
            }
            DecirTexto("pista", "", "¡Ya terminaste aquí! Usa el Mapa para ir al siguiente lugar.");
        }

        // ======================================================= Notas del diario
        void ArmarCampana() { AbrirAcertijo("final", () => { Poner("ok_final"); Refrescar(); CompletarNivel(3); FinalDelJuego(); }); }

        // Lo que Ana sabe del caso de la campana robada (evidencias y sospechosos), para el Diario.
        IEnumerable<string> NotasCaso()
        {
            if (B("ev_llave")) yield return "Evidencia: El Campanero tenía llave de la torre (la puerta no estaba forzada).";
            if (B("ev_pie")) yield return "Evidencia: dejó una huella chica de huarache junto a la estatua.";
            if (B("ev_azul")) yield return "Evidencia: un hilo azul quedó en la cuerda de la piñata.";
            if (B("ev_zurdo")) yield return "Evidencia: la carta la escribió alguien zurdo (la tinta está corrida).";
            if (B("vio_tomas")) yield return "Llaves de la torre: Don Tomás, Doña Lupita y Don Chuy.";
            if (B("vio_tomas")) yield return "Don Tomás, el sacristán: bajito, estola azul, enciende las velas con la derecha.";
            if (B("vio_chuy")) yield return "Don Chuy, el nevero: calza del 29; ropa blanca y turquesa.";
            if (B("vio_lupita")) yield return "Doña Lupita, la dulcera: bajita, rebozo azul, acomoda las cocadas con la izquierda.";
            if (B("ok_canal")) yield return "Don Ramiro, el alfarero: camisa azul; no tiene llave de la torre.";
            if (B("ok_caso")) yield return "<b>Caso resuelto:</b> El Campanero era Doña Lupita.";
        }

        IEnumerable<string> NotasDiario()
        {
            if (B("leida_placa")) yield return "Placa del atrio: «fachada terminada en MDCCLXXVIII» y «la madrugada del XVI del mes IX del año MDCCCX».";
            if (B("ok_candado")) yield return "Candado de la sacristía: 16 · 09.";
            if (B("ok_anillos")) yield return "El rayo del rosetón señaló el altar.";
            if (B("ok_nieves")) yield return "Nieves de Don Chuy: aguacate, mole, chicharrón, tequila, camarón.";
            var fichas = Enumerable.Range(1, 6).Where(i => Lleva("ficha" + i) || B("ok_criptex")).Select(i => "ALDAMA"[i - 1].ToString()).OrderBy(x => x).ToArray();
            if (E.nivel == 1 && fichas.Length > 0) yield return "Letras de las fichas de barro (revueltas): " + string.Join("  ", fichas);
            if (B("ok_cronologia")) yield return "Septiembre de 1810: aviso de Josefa Ortiz, llega Aldama, deciden, repican las campanas, salen a Atotonilco.";
            if (B("ok_criptex")) yield return "Criptex: ALDAMA.";
            if (B("ok_balanza")) yield return "Doña Lupita: «la clave de la cajita está en la bodega».";
            if (B("ok_hanoi")) yield return "Símbolos pintados en el piso de la bodega: una estrella, un sol y una luna.";
            if (B("leida_carta")) yield return "Carta revelada: «Clave: XVI entre IV».";
            if (B("ok_cesar")) yield return "Mensaje: LOS LIBROS GUARDAN LA COMBINACIÓN. ORDÉNALOS POR SU AÑO.";
            if (B("ok_libros")) yield return "Dígitos de los libros: 1 5 0 9 1 8.";
            if (B("ok_caja")) yield return "Ruta insurgente: Dolores, Atotonilco, San Miguel el Grande, Celaya, Guanajuato.";
        }
    }
}
