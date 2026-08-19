using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;

namespace NumuneKabul.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (!context.Institutions.Any(i => i.Name == "İzmir Şehir Hastanesi"))
        {
            context.Institutions.Add(new Institution
            {
                Name = "İzmir Şehir Hastanesi",
                Description = "İzmir Bölge Laboratuvarı Numune Kabul Birimi"
            });
            await context.SaveChangesAsync();
        }

        // Eğer veritabanında kullanıcı varsa, ana seed işlemi zaten yapılmış demektir
        if (context.Users.Any())
        {
            return; 
        }

        // 1. Örnek Kullanıcı (Admin)
        var adminUser = new User
        {
            Name = "Sistem Yöneticisi",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), // Default password for admin
            Role = "Admin"
            // InstitutionId null (Global yetki)
        };
        
        // Bu kullanıcının InstitutionId'si az sonra kurumlar oluşturulunca verilecek.
        var hospitalUser = new User
        {
            Name = "Ankara Şehir Hastanesi Kullanıcısı",
            Username = "hospital1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "Numune Kabul Personeli" // UY-01: "User" → "Numune Kabul Personeli"
        };
        
        // UY-02: Entegrasyon Servisi rolüne sahip servis kullanıcısı
        var integrationUser = new User
        {
            Name = "LIS Entegrasyon Servisi",
            Username = "lis_service",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("lis_service_password"),
            Role = "Entegrasyon Servisi"
        };
        
        await context.Users.AddRangeAsync(adminUser, hospitalUser, integrationUser);

        // 2. Örnek Kurumlar
        var devHospital = new Institution
        {
            Name = "Ankara Bilkent Şehir Hastanesi",
            Description = "Merkez Laboratuvar Numune Kabul Birimi"
        };
        var testClinic = new Institution
        {
            Name = "Test Polikliniği",
            Description = "Dış Servis"
        };
        await context.Institutions.AddRangeAsync(devHospital, testClinic);
        await context.SaveChangesAsync();
        
        // Şimdi ID'ler oluştuğu için hastane kullanıcısını kuruma bağlayalım
        hospitalUser.InstitutionId = devHospital.Id;
        context.Users.Update(hospitalUser);
        await context.SaveChangesAsync();

        // 3. Örnek Form Şablonu
        var defaultTemplate = new FormTemplate
        {
            InstitutionId = devHospital.Id,
            Name = "Standart Kan Numune Formu",
            Description = "Varsayılan OCR şablonu"
        };
        
        var pathologyTemplate = new FormTemplate
        {
            InstitutionId = devHospital.Id,
            Name = "Patoloji Laboratuvarı Tetkik İstem Formu",
            Description = "Patoloji örnekleri için detaylı form"
        };

        var biochemTemplate = new FormTemplate
        {
            InstitutionId = testClinic.Id,
            Name = "Biyokimya Talep Formu",
            Description = "Test Polikliniği biyokimya testleri"
        };

        var microbioTemplate = new FormTemplate
        {
            InstitutionId = testClinic.Id,
            Name = "Mikrobiyoloji Örnek Formu",
            Description = "Test Polikliniği mikrobiyoloji kültür testleri"
        };

        await context.FormTemplates.AddRangeAsync(defaultTemplate, pathologyTemplate, biochemTemplate, microbioTemplate);
        await context.SaveChangesAsync();

        // 4. Şablon Alanları (OCR'da taranacak hedefler)
        var fields = new List<TemplateField>
        {
            // Standart Kan
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "TC Kimlik",           DataType = "string", Regex = @"T\.?C\.?\s*K.ML.K\s*N[O0\.]*[^\d]*(\d{11})", OrderNo = 1 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Hasta Adı",            DataType = "string", Keyword = "Hasta Adı",           Required = true,  OrderNo = 2 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Hasta Soyadı",         DataType = "string", Keyword = "Hasta Soyadı",        Required = false, OrderNo = 3 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Numune Türü",          DataType = "string", Keyword = "Numune Türü",         Required = false, OrderNo = 4 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Tarih",               DataType = "date",   Keyword = "Tarih",               Required = false, OrderNo = 5 },

            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Protokol No",         DataType = "string", Keyword = "Protokol No",         Required = false, OrderNo = 6 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Test Adı",            DataType = "string", Keyword = "Test Adı",            Required = false, OrderNo = 7 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Numune Kabul Tarihi", DataType = "date",   Keyword = "Kabul Tarihi",        Required = false, OrderNo = 8 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Açıklama",            DataType = "string", Keyword = "Açıklama",            Required = false, OrderNo = 9 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Kurum",               DataType = "string", Keyword = "Kurum",               Required = false, OrderNo = 10 },
            new TemplateField { TemplateId = defaultTemplate.Id, FieldName = "Numune Barkodu",      DataType = "string", Keyword = "Barkod",              Required = false, OrderNo = 11 },

            // Patoloji (Forma göre güncellendi)
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Kabul No", Keyword = "Kabul No", DataType = "string", OrderNo = 1 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "TC Kimlik No", Keyword = "T.C.KİMLİK NO", DataType = "string", Regex = @"(\d{11})", OrderNo = 2 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Hasta Adı Soyadı", Keyword = "HASTANIN ADI SOYADI", DataType = "string", OrderNo = 3 },
            // UY-05: Hasta Adı ve Soyadı ayrı alanlar olarak da tanımlandı
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Hasta Adı",         Keyword = "HASTA ADI",          DataType = "string", Required = true,  OrderNo = 31 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Hasta Soyadı",      Keyword = "HASTA SOYADI",       DataType = "string", Required = false, OrderNo = 32 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Arşiv No", Keyword = "Arşiv No", DataType = "string", OrderNo = 4 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Doğum Tarihi", Keyword = "DOĞUM TARİHİ", DataType = "date", OrderNo = 5 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Adres Tel", Keyword = "ADRES/TEL", DataType = "string", OrderNo = 6 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Materyalin Alındığı Tarih", Keyword = "MATERYALİN ALINDIĞI TARİH", DataType = "date", OrderNo = 7 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Cinsiyeti", Keyword = "CİNSİYETİ", DataType = "string", OrderNo = 8 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Gönderen Doktor", Keyword = "GÖNDEREN DOKTOR", DataType = "string", OrderNo = 9 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Gönderen Bölüm", Keyword = "GÖNDEREN BÖLÜM", DataType = "string", OrderNo = 10 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Klinik Ön Tanı", Keyword = "KLİNİK ÖN TANI", DataType = "string", OrderNo = 11 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Klinik Öykü", Keyword = "Klinik Öykü ve Fizik Muayene Bulguları", DataType = "string", OrderNo = 12 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Alındığı Organ", Keyword = "Alındığı Organ", DataType = "string", OrderNo = 13 },
            new TemplateField { TemplateId = pathologyTemplate.Id, FieldName = "Alınış Şekli", Keyword = "Alınış Şekli", DataType = "string", OrderNo = 14 },
            
            // Biyokimya
            new TemplateField { TemplateId = biochemTemplate.Id, FieldName = "Barkod No", DataType = "string" },
            new TemplateField { TemplateId = biochemTemplate.Id, FieldName = "Test Listesi", DataType = "string" },

            // Mikrobiyoloji
            new TemplateField { TemplateId = microbioTemplate.Id, FieldName = "Örnek Alım Zamanı", DataType = "datetime" },
            new TemplateField { TemplateId = microbioTemplate.Id, FieldName = "Kültür Tipi", DataType = "string" }
        };
        await context.TemplateFields.AddRangeAsync(fields);
        await context.SaveChangesAsync();
    }
}
