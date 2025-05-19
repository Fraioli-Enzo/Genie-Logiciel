using System;
using System.Threading;

namespace WpfApp1;

public static class CryptoSoft
{
    private static readonly Mutex mutex = new(false, "CryptoSoft_Instance");

    /// <summary>
    /// Méthode publique exposée pour chiffrer/déchiffrer un fichier avec une clé.
    /// Retourne le temps en millisecondes, ou -99 en cas d'erreur.
    /// </summary>
    public static int RunEncryption(string path, string key)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(key))
            return -99;

        try
        {
            var fileManager = new FileManager(path, key);
            return fileManager.TransformFile();
        }
        catch
        {
            return -99;
        }
    }
}
