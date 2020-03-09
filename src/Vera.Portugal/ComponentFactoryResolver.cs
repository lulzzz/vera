using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Vera.Concurrency;
using Vera.Dependencies;

namespace Vera.Portugal
{
    public class ComponentFactoryResolver : IComponentFactoryResolver
    {
        private readonly ILocker _locker;

        public ComponentFactoryResolver(ILocker locker)
        {
            _locker = locker;
        }

        public IComponentFactory Resolve(Account account)
        {
            var privateKey = account.Configuration["PrivateKey"];

            using (var sr = new StringReader(Encoding.ASCII.GetString(Convert.FromBase64String(privateKey))))
            {
                var reader = new PemReader(sr);
                var o = reader.ReadPemObject() as AsymmetricCipherKeyPair;

            }
            

            using (var reader = File.OpenText(@"c:\myprivatekey.pem")) // file containing RSA PKCS1 private key
    keyPair = (AsymmetricCipherKeyPair) new PemReader(reader).ReadObject(); 

            // account.Configuration

            // TODO(kevin): get the RSA key and pass it to the factory
            // TODO(kevin): use account.Configuration to get the RSA key

            return new ComponentFactory(_locker, RSA.Create());
        }

        public string Name => "PT";
    }
}