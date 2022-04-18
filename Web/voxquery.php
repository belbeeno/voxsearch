<?php
header("Access-Control-Allow-Origin: https://v6p9d9t4.ssl.hwcdn.net");
function constructQuery($author, $and, $or, $not, $isSong, $isMorshu, $isGrant) {
  // Build against values
  $andCount = 0;
  $orCount = 0;
  $notCount = 0;
  $against = "\"";
  foreach ($and as &$iter) {
    if (!empty($iter)) {
      $against .= "+" . $iter . " ";
      $andCount++;
    }
  }
  foreach ($or as &$iter) {
    if (!empty($iter)) {
      $against .= $iter . " ";
      $orCount++;
    }
  }
  foreach ($not as &$iter) {
    if (!empty($iter)) {
      $against .= "-" . $iter . " ";
      $notCount++;
    }
  }
  $against .= "\"";

  $query = "SELECT * FROM `voxes` WHERE ";
  $isFirstWhereVox = true;

  if (!empty($author)) {
    if (!$isFirstWhereVox) $query .= "AND ";
    $query .= "`author` = \"" . $author . "\" ";
    $isFirstWhereVox = false;
  }

  if (!empty($isSong) || !empty($isMorshu) || !empty($isGrant) || $andCount > 0 || $orCount > 0 || $notCount > 0) {
    if (!$isFirstWhereVox) $query .= "AND ";
    $query .= "`id` in (SELECT `id` FROM `vox_meta` WHERE ";
    $isFirstWhereVox = false;
    $isFirstWhereMeta = true;
    if (!empty($isSong)) {
      if (!$isFirstWhereMeta) $query .= "AND ";
      $query .= "`has_song` = " . $isSong . " ";
      $isFirstWhereMeta = false;
    }

    // why is this getting applied to the query as true, when I don't mention it in index?
    if (!empty($isMorshu)) {
      if (!$isFirstWhereMeta) $query .= "AND ";
      $query .= "`has_morshu` = " . $isMorshu . " ";
      $isFirstWhereMeta = false;
    }

    if (!empty($isGrant)) {
      if (!$isFirstWhereMeta) $query .= "AND ";
      $query .= "`has_grant` = " . $isGrant . " ";
      $isFirstWhereMeta = false;
    }

    if ($andCount > 0 || $orCount > 0 || $notCount > 0) {
      if (!$isFirstWhereMeta) $query .= "AND ";
      $query .= "MATCH(`indexed_content`) AGAINST( " . $against . " IN BOOLEAN MODE)";
      $isFirstWhereMeta = false;
    }
    $query .= ")";
  }

  $query .= ";";
  return $query;
}

require_once("secret/voxquery_config.php");


$author = $_GET['author'];
$isSong = null;
if (!empty($_GET['s'])) {
  $isSong = ($_GET['s'] == "true" ? "true" : "false");
}
$isMorshu = null;
if (!empty($_GET['m'])) {
  $isMorshu = ($_GET['m'] == "true" ? "true" : "false");
}
$isGrant = null;
if (!empty($_GET['g'])) {
  $isGrant = ($_GET['g'] == "true" ? "true" : "false");
}

$latestQuery = $_GET['latest'];

$andQuery = explode(";", $_GET['qa']);
$orQuery = explode(";", $_GET['qo']);
$notQuery = explode(";", $_GET['qn']);

$hasTermQuery = false;
foreach ($andQuery as &$iter) {
  if (!empty($iter)) {
    $hasTermQuery = true;
  }
}
if (!$hasTermQuery) {
  foreach ($orQuery as &$iter) {
    if (!empty($iter)) {
      $hasTermQuery = true;
    }
  }
}
if (!$hasTermQuery) {
  foreach ($notQuery as &$iter) {
    if (!empty($iter)) {
      $hasTermQuery = true;
    }
  }
}

if ($latestQuery) {
  mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
  try {
    $conn = new mysqli("vox.belbeeno.com", $USERNAME, $PASSWORD, "voxsearch");
    if ($conn) {
      if ($conn->connection_error) {
        die("Connection failed: " . $conn->connect_error);
      }

      $result = $conn->query("SELECT `log_id` FROM `voxes` ORDER BY `date` DESC LIMIT 1;")->fetch_row();
      echo $result[0];
    }
  } catch (Exception $e) {
    echo 'Sorry, could not connect to the DB.  Please try again later.  Reason: ' .$e->getMessage(). '<br\>';
  }
}
else if (is_null($author) && !$hasTermQuery && is_null($isSong) && is_null($isMorshu) && is_null($isGrant)) {
  echo "No query.  Ignoring.";
}
else {
  //echo "Connecting to server...<br/>";
  mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
  try {
    $conn = new mysqli("vox.belbeeno.com", $USERNAME, $PASSWORD, "voxsearch");
  } catch (Exception $e) {
    echo 'Sorry, could not connect to the DB.  Please try again later.  Reason: ' .$e->getMessage(). '<br\>';
    exit();
  }

  if ($conn) {
    if ($conn->connection_error) {
      die("Connection failed: " . $conn->connect_error);
    }

    $query = constructQuery($author, $andQuery, $orQuery, $notQuery, $isSong, $isMorshu, $isGrant);
    $result = $conn->query($query);

    class VoxEntry {
      public $id;
      public $author;
      public $log_id;
      public $content;
    }

    $count = $result->num_rows;
    $voxes = array();
    echo "{ \"content\": [";
    if ($count > 0) {
      while ($row = $result->fetch_assoc()) {
        echo json_encode($row);
        //echo "<p><b>author</b>: " . $row["author"] . ", <b>content</b>: " . $row["content"] . "<br>";
        $count--;
        if ($count != 0) {
          echo ",";
        }
      }
    }
    echo "]}";
  }
}

?>
