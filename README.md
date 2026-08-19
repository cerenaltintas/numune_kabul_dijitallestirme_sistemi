# Numune Kabul Dijitalleştirme Sistemi

Numune Kabul Dijitalleştirme Sistemi, hastanelerde ve laboratuvarlarda kağıt ortamında bulunan **Hasta Numune Kabul Formlarını** dijital ortama aktarmak için geliştirilmiş kapsamlı bir kurumsal web uygulamasıdır. 

Sistem; yüklenen PDF formatındaki formları işler, **Tesseract OCR** motoru ile metinleri okur, akıllı şablonlar kullanarak istenilen hasta ve tetkik verilerini (Ad, Soyad, TC Kimlik, Doğum Tarihi, Testler vb.) ayıklar. Kullanıcıların görsel bir arayüzde doğrulama yapmasına olanak tanır ve onaylanan verileri **XML formatında** LIS/HBYS gibi dış sistemlere gönderir.

---

## Temel Özellikler

- **Akıllı PDF İşleme:** 50MB'a kadar çok sayfalı PDF yükleme desteği ve Magic Byte doğrulama ile güvenli dosya kabulü.
- **Optik Karakter Tanıma (OCR):** Tesseract OCR entegrasyonu ile (Zonal Extraction ve Keyword Fallback) yüksek doğruluklu metin okuma.
- **Şablon Tabanlı Veri Çıkarımı:** Düzenlenebilir şablonlar sayesinde belgedeki ilgili alanların (Koordinat veya Regex bazlı) otomatik bulunması.
- **Görsel Doğrulama ve Düzenleme:** Çıkarılan verilerin PDF üzerinde renkli kutularla (bounding box) gösterilmesi ve kullanıcı tarafından manuel düzeltilebilmesi.
- **Kurumsal Entegrasyon:** Doğrulanan formların standart XML yapısında (HL7/FHIR genişletilebilir) Dış Sistemlere (Mock REST vb.) otomatik gönderimi ve hata durumunda *Retry (Yeniden Deneme)* mekanizması.
- **Güvenlik & Yetkilendirme:** JWT tabanlı kimlik doğrulama, Role-Based Access Control, IDOR korumalı Multi-Tenant yapı (Kurum bazlı izolasyon) ve Session güvenliği.
- **Merkezi Loglama (Audit):** Sistemdeki her adımın (okuma, manuel düzeltme, gönderim) kim tarafından, ne zaman yapıldığının Serilog ve veritabanı aracılığıyla izlenmesi.

---

## Mimari ve Teknolojiler

Proje, kurumsal yazılım standartlarına uygun olarak **Clean Architecture** (Temiz Mimari) ve **SOLID** prensipleri gözetilerek 6 katmanlı bir yapıda tasarlanmıştır:

### Katmanlar
1. **NumuneKabul.Domain:** Çekirdek katman (Entity'ler, Enum'lar, Core Interface'ler).
2. **NumuneKabul.Application:** İş mantığı, DTO'lar, Servis arayüzleri, Doğrulamalar.
3. **NumuneKabul.Infrastructure:** Dış bağımlılıklar (EF Core, Tesseract OCR, File Storage, Adapter implementasyonları).
4. **NumuneKabul.API:** RESTful endpoint'leri sunan, JWT korumalı Backend (Kestrel).
5. **NumuneKabul.Web:** Kullanıcı arayüzünü (MVC) sunan, API ile haberleşen Frontend.
6. **NumuneKabul.Tests:** Birim ve Entegrasyon testleri (xUnit, Moq, FluentAssertions).

### Kullanılan Teknolojiler
- **Backend:** .NET 9, ASP.NET Core Web API & MVC
- **Veritabanı:** Entity Framework Core (Geliştirme: SQLite, Canlı: MSSQL)
- **OCR Motoru:** Tesseract OCR (tessdata), System.Drawing
- **PDF İşleme:** PdfiumViewer, PdfPig
- **Loglama:** Serilog
- **Güvenlik:** BCrypt (Şifreleme), JWT Bearer Token

---

## Tasarım Kalıpları (Design Patterns)

Sistemin esnekliğini ve sürdürülebilirliğini sağlamak için çeşitli tasarım kalıpları kullanılmıştır:
- **Unit of Work & Generic Repository:** Veritabanı işlemlerinin tek bir transaction üzerinden atomik yürütülmesi.
- **Strategy Pattern:** OCR çıkarma yöntemlerinin (Zonal, Regex) ve XML formatlarının esnek olarak değiştirilebilmesi.
- **Builder Pattern:** Karmaşık XML belgelerinin adım adım okunaklı şekilde oluşturulması.
- **Adapter Pattern:** Dış entegrasyon (LIS/HBYS) katmanının soyutlanarak, sistemin geri kalanından izole edilmesi.

---

## Kurulum ve Çalıştırma

### Önkoşullar
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Tesseract dili dosyaları (`tessdata/tur.traineddata` vb.) uygulamanın kök dizininde olmalıdır.

### Adımlar

1. **Depoyu Klonlayın:**
   ```bash
   git clone https://github.com/kullaniciadi/NumuneKabulSistemi.git
   cd NumuneKabulSistemi
   ```

2. **Veritabanını Oluşturun:**
   Uygulama geliştirme aşamasında SQLite kullanır. Veritabanı ve Seed verileri ilk çalışmada otomatik oluşturulur. Ancak manuel uygulamak isterseniz:
   ```bash
   cd NumuneKabul.API
   dotnet ef database update --project ../NumuneKabul.Infrastructure
   ```

3. **API'yi Başlatın:**
   ```bash
   cd NumuneKabul.API
   dotnet run
   ```
   *API `http://localhost:5151` portunda çalışacaktır (Swagger UI mevcuttur).*

4. **Web (Frontend) Uygulamasını Başlatın:**
   Ayrı bir terminal penceresinde:
   ```bash
   cd NumuneKabul.Web
   dotnet run
   ```
   *Web arayüzü `http://localhost:5001` portunda çalışacaktır.*

### Test Kullanıcıları
Sistem ayağa kalktığında otomatik olarak (Seed Data) test kullanıcıları oluşturulur:
- **Kullanıcı Adı:** `admin` / **Şifre:** `Admin123!` (Sistem Yöneticisi)
- **Kullanıcı Adı:** `personel` / **Şifre:** `Pers123!` (Kurum Personeli - Sadece kendi kurumunu görür)

---

## Testler

Proje kapsamlı bir test altyapısına sahiptir. Tüm birim (Unit) ve entegrasyon testlerini çalıştırmak için ana dizinde şu komutu kullanın:

```bash
dotnet test
```

---

## Lisans

Bu proje **MIT Lisansı** ile lisanslanmıştır. Daha fazla bilgi için `LICENSE` dosyasına bakabilirsiniz.
