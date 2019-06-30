function selectDirClick(elem){
   $("span").filter(".selected").removeClass("selected");
   $(elem).addClass("selected");
   var parent = $(elem).parent().get(0);
   switch(parent.className.replace(" ","")){
      case "leaf":
         navigateDirClick(elem);
         break;
      case "closed":
         parent.className = "opened";
         break;
      case "opened":
         parent.className = "closed";
         break;
   }
}

function navigateDirClick(elem){
   var parent = $(elem).parent().get(0);
   window.parent.contentFrame.location = "./content/" + parent.id;
}

function ChangeContentFromID(id){
   var menuJquery = window.parent.menuFrame.$;
   var elem = menuJquery("#"+GetEscapedString(id));
   elem = elem.children().get(0);
   if(elem != null){
      menuJquery("span").filter(".selected").removeClass("selected");
      menuJquery(elem).addClass("selected");
      var parent = menuJquery(elem).parent().get(0);
      if(parent.className.replace(" ","") == "leaf"){
         window.parent.contentFrame.location = "../content/" + parent.id;
      }
   }
}

function GetEscapedString(str){
   return str.replace(".","\\.");
}