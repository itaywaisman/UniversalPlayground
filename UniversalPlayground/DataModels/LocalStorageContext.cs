// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.EntityFrameworkCore;
using UniversalPlayground.Helpers;

namespace UniversalPlayground.DataModels
{
    // documnetation is here: https://docs.microsoft.com/en-us/ef/core/get-started/uwp/getting-started
    // under package manager console, use Add-Migration <nameofchange> every time this class updated
    public class LocalStorageContext : DbContext
    {
        private static AsyncInitilizer<LocalStorageContext> _initializer = new AsyncInitilizer<LocalStorageContext>();

        static LocalStorageContext()
        {
            _initializer.InitializeWith(CheckForDatabase);
        }

        public static void CheckMigrations()
        {
            _initializer.CheckInitialized();
            using (var db = new LocalStorageContext())
            {
                db.Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=buildcast.db");
        }

        private static async Task CheckForDatabase()
        {
            var mainDbFileName = "buildcast.db";
            var mainDbAssetPath = $"ms-appx:///Assets/{mainDbFileName}";

            var data = Windows.Storage.ApplicationData.Current.LocalFolder;

            var exists = await data.TryGetItemAsync(mainDbFileName);

            if (exists == null)
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(mainDbAssetPath)).AsTask().ConfigureAwait(false);
                var database = await file.CopyAsync(data).AsTask().ConfigureAwait(false);
            }
        }
    }
}
