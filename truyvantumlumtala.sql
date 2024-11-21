select *
from answer_response
where surveyID = 2
order by(id) desc

SELECT *
FROM answer_response
WHERE surveyID = 2
AND id_sv IN (
    SELECT id_sv
    FROM answer_response
    WHERE surveyID = 2
    GROUP BY id_sv
    HAVING COUNT(*) > 1
)
ORDER BY id_sv, id DESC;



SELECT ar.*
FROM answer_response ar
WHERE ar.surveyID = 2
AND ar.id IN (
    SELECT MIN(ar_inner.id) 
    FROM answer_response ar_inner
    WHERE ar_inner.surveyID = 2
    GROUP BY ar_inner.id_sv
    HAVING COUNT(*) > 1
)
ORDER BY ar.id_sv;
SELECT id_sv, COUNT(*) AS DuplicateCount
FROM answer_response
WHERE surveyID = 2
GROUP BY id_sv
HAVING COUNT(*) > 1
ORDER BY DuplicateCount DESC;

select *
from users
order by(id_users) desc

DELETE FROM answer_response
WHERE id IN (
    14223, 14224, 14225, 14227, 14228, 14232, 14233, 14234, 14235, 14236,
    14238, 14239, 14240, 14241, 14242, 14243, 14244, 14245, 14247, 14248,
    14249, 14250, 14251, 14252, 14253, 14254, 14256, 14257, 14258, 14259
);


delete from users
where id_users = 133
select sv.*,l.ma_lop,ct.ten_ctdt
from sinhvien sv, lop l,ctdt ct
where sv.id_lop = l.id_lop
and l.id_ctdt = ct.id_ctdt
and ten_ctdt = N'QUẢN TRỊ KINH DOANH (THS)'
and sv.ngaycapnhat != N'1732211402'

select l.ma_lop,ct.ten_ctdt
from  lop l,ctdt ct
where l.id_ctdt = ct.id_ctdt
and ct.ten_ctdt = N'KẾ TOÁN (THS)'

select *
from answer_response aw, ctdt ct
where aw.id_ctdt = ct.id_ctdt
and ct.ten_ctdt = N'NGÔN NGỮ ANH (THS)'

select *
from sinhvien
where ma_sv = N'238340201010'

select *
from sinhvien
where id_sv = 1098
DELETE FROM sinhvien
WHERE id_sv IN (
    426, 428, 432, 434, 439, 442, 453, 455, 462, 464, 
    469, 476, 481, 486, 487, 548, 563, 577, 1411, 1412, 
    1413, 1414, 1415, 1416, 1417, 1418, 1419, 1420, 1421, 
    1422, 1423, 1424, 1425, 1426, 1427, 1428, 1429, 1430, 
    1431, 1432, 1433, 1434, 1435, 1436, 1437, 1438, 1439, 
    1440, 1441, 1442, 1443, 1444, 1445, 1446, 1447, 1448, 
    1449, 1450, 1451, 1452, 1453, 1454, 1455, 1456, 1457, 
    1458, 1459, 1460, 1461, 1462, 1463, 1464, 1465, 1466, 
    1467, 1468, 1469, 1470, 1471, 1472, 1473, 1474, 1475, 
    1476, 1477, 1478, 1479, 1480, 1481, 1482, 1483, 1484, 
    1485, 1486, 1487, 1488, 1489, 1490, 1491
);


delete from answer_response
where id = 203

