using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace core.services.aes;

public class 
AesService {
  private readonly byte[] default_key_;

  public 
  AesService(string default_key) {
    if (string.IsNullOrEmpty(default_key)) {
      throw new ArgumentNullException(nameof(default_key));
    }

    default_key_ = Encoding.UTF8.GetBytes(
      default_key.PadRight(32).Substring(0, 32));
  }

  public string 
  Encrypt(string plain_text, string custom_key) {
    if (string.IsNullOrEmpty(plain_text)) {
      return plain_text;
    }

    byte[] key = default_key_;
    if (!string.IsNullOrWhiteSpace(custom_key)) {
      key = prepare_key(custom_key);
    }

    using (Aes aes = Aes.Create()) {
      aes.Key = key;
      aes.GenerateIV();

      using (MemoryStream ms = new MemoryStream()) {
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (CryptoStream cs = new CryptoStream(
          ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write)) {
          using (StreamWriter sw = new StreamWriter(cs)) {
            sw.Write(plain_text);
          }
        }

        return Convert.ToBase64String(ms.ToArray());
      }
    }
  }

  public string 
  Decrypt(string cipher_text, string custom_key) {
    if (string.IsNullOrEmpty(cipher_text)) {
      return cipher_text;
    }

    byte[] key = default_key_;
    if (!string.IsNullOrWhiteSpace(custom_key)) {
      key = prepare_key(custom_key);
    }
    
    byte[] full_cipher = Convert.FromBase64String(cipher_text);

    using (Aes aes = Aes.Create()) {
      aes.Key = key;

      byte[] iv = new byte[aes.BlockSize / 8];
      Array.Copy(full_cipher, 0, iv, 0, iv.Length);
      aes.IV = iv;

      using (MemoryStream ms = new MemoryStream(
        full_cipher, iv.Length, full_cipher.Length - iv.Length)) {
        using (CryptoStream cs = new CryptoStream(
          ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read)) {
          using (StreamReader sr = new StreamReader(cs)) {
            return sr.ReadToEnd();
          }
        }
      }
    }
  }

  private byte[] 
  prepare_key(string key) {
    return Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
  }
}
