Food Matching Game
Bu proje, Unity ile geliştirilmiş bir yemek eşleştirme ve skor toplama oyunudur. Sahnede beliren rastgele yemekleri toplayarak veya çeşitli skill tuşlarını kullanarak puan kazanabilirsiniz. Ayrıca UI üzerinden güncel skorunuzu takip edebilirsiniz.

Nasıl Çalışır?
Yemeklerin Sahneye Eklenmesi:

Oyun başladığında belirli sayıda yiyecek (etiketi “Moveable” olan objeler) rastgele konumlarda sahneye eklenir.
Bu yiyecekler, bir konteynır (sandık) ile etkileşime girer ve içeri alındığında sabit bir noktaya yerleştirilir.
Yemek Eşleştirme Mekanizması:

Konteynırın içine aynı anda iki yemek girdiğinde, bu yemeklerin isimlerinin ilk harfi karşılaştırılır.
Eğer aynı harfle başlıyorlarsa:
İki yemek yok edilir.
Harfin puan değeri kadar skor elde edersiniz (örneğin ‘S’ harfi 200 puan).
Eğer farklı harfle başlıyorlarsa:
Yemekler konteynırdan çıkarılır ve sahnenin başka bir noktasına geri gönderilir.
Skor ve UI:

Topladığınız veya yok ettiğiniz her yemekten aldığınız puan, ekrandaki Skor (Score) alanında güncellenir.
TextMesh Pro ile oluşturulan bir metin (UI) üzerinden güncel skor görüntülenir.
Skill Tuşları
Oyun içerisinde 3 özel skill tuşu bulunur: Z, X ve C.

Z Skill
Görevi: 10 saniye boyunca kazanılan puanı 2 katına çıkarır.
Nasıl Kullanılır?:
Z tuşuna basıldığında etkinleşir (cooldown süresi bitmişse).
Aktif olduğu 10 saniye boyunca, yemek eşleştirmeden veya diğer puan kazandıran aksiyonlardan 2x puan alınır.
10 saniye sonunda bu çarpan biter ve 10 saniyelik bir cooldown başlar.
X Skill
Görevi: Sahneye 2 adet Steak (eski adıyla “Golden Apple”) ekler.
Nasıl Kullanılır?:
X tuşuna basıldığında, sahnede rastgele konumlarda 2 adet Steak (prefab) oluşturulur.
Skill kullanıldıktan sonra 15 saniye boyunca tekrar kullanılamaz (cooldown).
C Skill
Görevi: Sahnedeki tüm yiyecekleri yok eder ve her yok edilen obje için 30 puan kazandırır.
Nasıl Kullanılır?:
C tuşuna basıldığında etkinleşir (cooldown süresi uygunsa).
Sahnedeki tüm spawn edilmiş yiyecek objeleri yok edilir.
Yok edilen her obje size 30 puan kazandırır.
Skill etkinleştikten sonra 60 saniye cooldown başlar ve bu süre dolmadan yeniden kullanılamaz.