

## NUMUNE KABUL DİJİTALLEŞTİRME SİSTEMİ
OCR Tabanlı Doküman İşleme ve LIS/HBYS Entegrasyonu
MVP Analiz ve Yazılım Gereksinimleri Dokümanı

## 1. Proje Amacı
Laboratuvar numune kabul süreçlerinde farklı kurumlara ait PDF başvuru formları
kullanılmaktadır. Bu formlar üzerinde yer alan bilgiler, numune kabul personeli tarafından
manuel olarak Laboratuvar Bilgi Yönetim Sistemi (LIS) veya Hastane Bilgi Yönetim Sistemi
(HBYS)'ne girilmektedir.
Bu süreç;
- Zaman kaybına,
- Veri giriş hatalarına,
- Personel bağımlılığına,
- İş yükünün artmasına,
- Standart olmayan veri girişine
neden olmaktadır.
Bu projenin amacı; PDF formlarındaki bilgilerin OCR teknolojisi ile okunması, şablon bazlı
kurallar kullanılarak anlamlı alanlara ayrıştırılması, kullanıcı tarafından doğrulanması ve standart
XML formatında saklanarak LIS/HBYS sistemlerine aktarılmasını sağlayan bir ara katman
uygulaması geliştirmektir.
Bu sürüm tamamen OCR + Şablon + Kural Tabanlı (Rule Based) olarak geliştirilecek olup yapay
zekâ kullanımı MVP kapsamı dışındadır.

## 2. Proje Hedefleri
Sistem aşağıdaki işlemleri gerçekleştirecektir.
- PDF yükleme
- OCR ile metin çıkarma
- Kuruma ait form şablonunu seçme
- Şablondaki alanları otomatik eşleştirme
- Güven seviyesi düşük alanları işaretleme
- Kullanıcının eksik alanları düzeltmesini sağlama
- Düzeltilmiş veriyi XML olarak saklama
- LIS/HBYS sistemlerine aktarılabilecek standart veri üretme


## 3. Proje Kapsamı
## Dahil
- PDF yükleme
- OCR işlemi
- Şablon yönetimi
- Regex/kural tabanlı alan çıkarımı
- Manuel doğrulama ekranı
- XML üretimi
## • Loglama
## • REST API
- SQLite ve MSSQL desteği
## Hariç
MVP kapsamında aşağıdaki özellikler geliştirilmeyecektir.
- Yapay zekâ destekli alan çıkarımı
- LLM kullanımı
- El yazısı tanıma (HTR)
- Mobil uygulama
## • Dashboard
- Gelişmiş raporlama
- Gerçek HBYS entegrasyonu (Mock servis kullanılacaktır)
- HL7/FHIR desteği (ileriki sürümlerde)

## 4. Sistem Mimarisi
## Kullanıcı

## │

PDF Yükleme

## │


PDF İşleme Servisi

## │

## OCR

## │

## Şablon Seçimi

## │

## Kural / Regex Motoru

## │

## Alanların Bulunması

## │

## Kullanıcı Doğrulaması

## │

XML Oluşturucu

## │

## Integration Adapter

## │


## LIS / HBYS

## 5. Teknoloji Altyapısı
## Katman Teknoloji
Backend ASP.NET Core Web API
Arayüz ASP.NET Core MVC veya Razor Pages
ORM Entity Framework Core
Veritabanı SQLite (Geliştirme), MSSQL (Canlı)
OCR Tesseract OCR (servis arayüzü üzerinden)
PDF İşleme PDFium veya PdfPig
XML System.Xml
## Loglama Serilog
Kimlik Doğrulama JWT Authentication
## API REST

## 6. Tasarım Prensipleri
Sistem modüler geliştirilecektir.
OCR, alan çıkarımı ve entegrasyon servisleri birbirinden bağımsız olacaktır.
Böylece ileride farklı OCR motorları veya LLM tabanlı servisler sisteme minimum değişiklikle
eklenebilecektir.
## Örneğin;
- Tesseract yerine başka OCR motoru
- Regex yerine LLM tabanlı alan çıkarımı
- REST yerine HL7/FHIR entegrasyonu
eklenebilir olacaktır.

## 7. Kullanıcı Rolleri
## Admin
- Kurum yönetimi

- Form şablonu yönetimi
- Kullanıcı yönetimi
- Sistem ayarları
## Numune Kabul Personeli
- PDF yükleme
- OCR başlatma
- Alan doğrulama
- Manuel düzeltme
- XML oluşturma
- Entegrasyona gönderme
## Entegrasyon Servisi
- XML alma
- LIS/HBYS sistemine gönderme

## 8. İş Akışı
- Kullanıcı PDF yükler.
- Sistem OCR işlemini başlatır.
- OCR metni oluşturulur.
- Kuruma ait form şablonu seçilir.
- Şablondaki kurallar uygulanır.
- Alanlar otomatik bulunur.
- Güven seviyesi hesaplanır.
- PDF üzerinde ilgili alanlar işaretlenir.
- Kullanıcı eksik alanları düzeltir.
- XML oluşturulur.
- XML veritabanına kaydedilir.
- Entegrasyon servisine gönderilir.

## 9. Fonksiyonel Gereksinimler
PDF Yönetimi
- PDF yükleme

- PDF görüntüleme
- PDF silme
- Çok sayfalı PDF desteği

## OCR
- PDF'den metin çıkarılması
- Sayfa bazlı OCR
- Ham OCR metninin saklanması
- OCR tekrar çalıştırma

## Form Şablonları
Her kurum için bir veya daha fazla form şablonu tanımlanabilecektir.
## Şablon;
- Alan adı
## • Regex
- Anahtar kelime
- Zorunlu alan
- Veri tipi
bilgilerini içerecektir.

## Alan Çıkarma
MVP kapsamında aşağıdaki alanlar desteklenecektir.
## • Hasta Adı
## • Hasta Soyadı
- T.C. Kimlik No
## • Doğum Tarihi
## • Cinsiyet
## • Kurum
## • Doktor
## • Protokol No
## • Numune Barkodu

## • Numune Türü
## • Test Adı
## • Numune Kabul Tarihi
## • Açıklama
Her alan için;
- OCR değeri
- Düzeltilmiş değer
- Güven skoru
- Sayfa numarası
## • Koordinatlar
## • Durum
saklanacaktır.

PDF İşaretleme
Sistem OCR sonucu bulunan alanları PDF üzerinde gösterecektir.
## Renkler;
## Yeşil
- Güven yüksek
## Sarı
- Kontrol edilmeli
## Kırmızı
## • Okunamadı
Kullanıcı işaretli alanlara tıklayarak düzenleme yapabilecektir.

## Manuel Düzenleme
## Kullanıcı;
- Klavye ile veri girebilir.
- Kopyala-yapıştır yapabilir.
- OCR sonucunu değiştirebilir.
Her değişiklik Audit Log'a yazılacaktır.


## XML
Her belge için XML üretilecektir.
XML içerisinde;
- OCR metni
- Düzeltilmiş alanlar
- Güven skorları
## • Koordinatlar
- Belge bilgileri
saklanacaktır.

## Entegrasyon
İlk sürümde;
REST API üzerinden XML gönderilecektir.
İlerleyen sürümlerde;
## • HL7
## • FHIR
## • SOAP
- Dosya aktarımı
desteklenebilecektir.

## 10. Fonksiyonel Olmayan Gereksinimler
## Performans
- Ortalama OCR süresi 10 saniyenin altında olmalıdır.
- Aynı anda en az 10 kullanıcı desteklenmelidir.
## Güvenlik
## • HTTPS
- JWT Authentication
## • Role Based Authorization
## • Audit Log
## Loglama
Loglanacak işlemler;

- PDF yükleme
## • OCR
- Manuel düzeltmeler
- XML üretimi
## • Entegrasyon
## • Hatalar

## 11. Veritabanı Modeli
## Users
## • Id
## • Name
## • Username
- PasswordHash
## • Role
## Institutions
## • Id
## • Name
FormTemplates
## • Id
- InstitutionId
## • Name
## • Description
TemplateFields
## • Id
- TemplateId
- FieldName
## • Regex
## • Required
- DataType
- OrderNo
PdfDocuments

## • Id
- InstitutionId
- TemplateId
- FileName
- UploadDate
## • Status
OcrResults
## • Id
- PdfId
- RawText
ExtractedFields
## • Id
- PdfId
- FieldName
- RawValue
- CorrectedValue
## • Confidence
- PageNo
## • X
## • Y
## • Width
## • Height
## • Status
XmlArchives
## • Id
- PdfId
- XmlContent
- CreatedDate
IntegrationJobs
## • Id
- PdfId

## • Status
- RetryCount
AuditLogs
## • Id
- UserId
## • Action
## • Date
## • Description

- API Tasarımı
## PDF
POST /api/pdf/upload
GET /api/pdf/{id}
DELETE /api/pdf/{id}

## OCR
POST /api/ocr/start/{id}
GET /api/ocr/result/{id}

## Fields
GET /api/fields/{id}
PUT /api/fields/{id}

## XML
POST /api/xml/create/{id}
GET /api/xml/{id}

## Integration
POST /api/integration/send/{id}
GET /api/integration/status/{id}


- MVP Başarı Kriterleri
Bir sürümün tamamlanmış kabul edilmesi için aşağıdaki maddelerin eksiksiz çalışması gerekir.
- PDF yüklenebilmelidir.
- OCR metni çıkarılabilmelidir.
- Şablona göre alanlar otomatik bulunabilmelidir.
- Alanlar PDF üzerinde işaretlenebilmelidir.
- Kullanıcı manuel düzeltme yapabilmelidir.
- XML üretilebilmelidir.
- XML veritabanında saklanabilmelidir.
- Mock entegrasyon servisine gönderilebilmelidir.
- Tüm işlemler loglanmalıdır.

## 14. 30 İş Günlük Geliştirme Planı
## Gün Yapılacak İş
1 [TAMAMLANDI] Proje kurulumu, katmanlı mimari ve Git
2 [TAMAMLANDI] Veritabanı modeli ve EF Core
3 [TAMAMLANDI] SQLite ve MSSQL desteği
4 [TAMAMLANDI] PDF yükleme modülü
5 [TAMAMLANDI] PDF listeleme ve görüntüleme
6 [TAMAMLANDI] PDF sayfalarının görüntüye dönüştürülmesi
7 [TAMAMLANDI] OCR servis altyapısı
8 [TAMAMLANDI] Tesseract OCR entegrasyonu
9 [TAMAMLANDI] OCR çıktısının kaydedilmesi
10 [TAMAMLANDI] OCR sonucu görüntüleme ekranı
11 [TAMAMLANDI] Form şablonu modeli
12 [TAMAMLANDI] Şablon yönetim ekranı
13 [TAMAMLANDI] Regex/kural motorunun geliştirilmesi
14 [TAMAMLANDI] Alan çıkarma kurallarının uygulanması

## Gün Yapılacak İş
15 [TAMAMLANDI] ExtractedFields yapısının tamamlanması
16 [TAMAMLANDI] Güven skoru hesaplama
17 [TAMAMLANDI] PDF üzerinde koordinat gösterimi
18 [TAMAMLANDI] Renkli alan işaretleme
19 [TAMAMLANDI] Manuel düzenleme ekranı
20 [TAMAMLANDI] Audit Log sistemi
21 [TAMAMLANDI] XML üretimi
22 [TAMAMLANDI] XML arşivleme
23 [TAMAMLANDI] XML Mapping
24 [TAMAMLANDI] Entegrasyon adapter yapısı
25 [TAMAMLANDI] Mock REST servisine gönderim
26 [TAMAMLANDI] Entegrasyon kuyrugu
27 [TAMAMLANDI] Hata yönetimi ve yeniden gönderim
28 [TAMAMLANDI] Kullanıcı ve rol yönetimi
29 [TAMAMLANDI] Uçtan uca testler
30 [TAMAMLANDI] Dokümantasyon ve teslim

## 15. Gelecek Sürümler
Sistem mimarisi aşağıdaki geliştirmeleri destekleyecek şekilde tasarlanacaktır.
- Local LLM (Ollama vb.) ile alan çıkarımı
- Otomatik form tipi tanıma
- El yazısı tanıma (HTR)
- Barkod ve QR kod otomatik okuma
- Şablonsuz belge analizi
- HL7/FHIR entegrasyonu
- Gelişmiş raporlama
## • Dashboard
- OCR kalite analizi

- Aktif öğrenme (Human-in-the-loop)

## 16. Sonuç
Bu proje, laboratuvar numune kabul süreçlerinde manuel veri girişini azaltmayı amaçlayan, OCR
tabanlı ve şablon yönetimi üzerine kurulu bir dijitalleştirme çözümüdür. MVP sürümü herhangi
bir yapay zekâ bağımlılığı içermemektedir. Bunun yerine OCR, kurum bazlı form şablonları,
regex/kural tabanlı alan çıkarımı ve kullanıcı doğrulaması kullanılarak güvenilir ve sürdürülebilir
bir çözüm sunmaktadır.
Sistem; katmanlı mimarisi, servis tabanlı yaklaşımı ve genişletilebilir yapısı sayesinde gelecekte
farklı OCR motorlarının, yerel LLM çözümlerinin ve HL7/FHIR gibi sağlık bilişimi standartlarının
kolayca entegre edilebileceği bir altyapı sağlayacaktır.
