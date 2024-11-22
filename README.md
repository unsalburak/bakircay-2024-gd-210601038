# bakircay-2024-gd-210601038

Proje Özellikleri
Nesne Çekme Mekanizması: Bir nesne Zone’a yaklaştığında otomatik olarak yerine yerleştirilir.
Tek Nesne Kuralı: Zone aynı anda yalnızca bir nesne kabul eder.
Dinamik Trigger Yönetimi: Bir nesne Zone’a girdiğinde alanın Is Trigger özelliği kapatılır, nesne yok edildikten sonra tekrar açılır.
Fizik Etkileşimleri: Nesneler fizik kurallarına uygun şekilde hareket eder ve Zone üzerinde sabitlenir.
Zamanlı Yok Etme: Yerleşen nesne 3 saniye içinde yok edilir ve alan başka bir nesneyi kabul etmeye hazır hale gelir.

Ana Scriptler
AreaZone.cs:
Nesneleri Zone içine çekme, sabitleme ve yok etme işlemlerini yönetir.
DetectObject.cs:
Fare ile nesne hareketlerini algılar ve nesneleri fiziksel olarak taşır.
TouchManager.cs:
Dokunmatik ekran veya fare girdilerini işler.