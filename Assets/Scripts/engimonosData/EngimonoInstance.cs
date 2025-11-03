using System;

////////////////////////////////////////////////////////////////////////////////////////////
// esta clase representa una instancia concreta de un engimono
// solo almacena datos serializables que otros scripts pueden leer y modificar
// se usa como contenedor para referenciar un EngimonosData base (que define el engimono original, su nombre, descripcion, stats, etc) y marcar si el engimono fue comprado o no
// no queria tener un script tan cortito por si solo pero mejor dejarlo asi porque no vi ninguna forma buena de implementarlo en otro script
////////////////////////////////////////////////////////////////////////////////////////////

[Serializable] // permite que Unity muestre y guarde esta clase en el Inspector
public class EngimonoInstance
{
    // referencia a los datos base del engimono
    public EngimonosData data;

    // indica si el engimono fue comprado o desbloqueado
    // sirve para condicionar cosas como arrastre, uso, o visuales
    public bool comprado = true;
}