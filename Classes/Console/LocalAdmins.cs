using System.Collections.Generic;

namespace StupidTemplate.Classes.Console;

public class LocalAdmins
{
    // This menu template comes with an implementation of 'Console', the popular Gorilla Tag admin system.
    // You may remove console if you wish, or use it in your own menu.
    // You have to make admins mods yourself, but the admin system is here for you to use.
    // If you want to make yourself an admin on this menu you can add your player id and any name of your you want below.
    // An example placeholder has been left as a comment inside the Dictionary

    public static Dictionary<string, string> Admins = new()
    {
            //{"username", "player id"},
    };

    // Super admins have a higher level of power and have a different icon.
    // It's recommended you give yourself super admin
    // All you need to do is enter the *same* username as the one you put in the Admins Dictionary above
    public static string[] SuperAdmins =
    [
            //"username",
    ];
}