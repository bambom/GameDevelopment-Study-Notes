// frida -U  -f com.pwrd.lhj.aligames --no-pause -l C:\Users\pc\Desktop\unity_bootstrap.js
// 运行环境是mumu模拟器


Java.perform(function () {


function get_func_by_offset(module_name,offset){
    var module=Process.getModuleByName(module_name)
    var addr=module.base.add(offset);
    return new NativePointer(addr.toString());
}
var is_matched = false;
var so_path = null;
// dllopen  0x2101

    var func = get_func_by_offset('linker',0x2101  )    //针对网易mumu模拟器,非mumu模拟器需要修改offset的值

    //console.log('[+] dlopen '+ func.toString())
    Interceptor.attach(func, {
        onEnter: function (args) {
            so_path =  Memory.readCString(args[0])
            //console.log('[*] ' + so_path);
        },
        onLeave: function (retval) {
            if(so_path == null || so_path.indexOf('libil2cpp.so') == -1 || is_matched == true){
                return;
            }
            is_matched = true
            console.log('[*] '+so_path)
            hookSet()
            return;
        }
    });

function hookSet(){
    console.log("********hookSet start*******")
    var get_nUnitType = get_func_by_offset("libil2cpp.so",0x3CF93C8);  //[Address(RVA="0x3CF93C8", Offset="0x3CF93C8")]public EUnitType get_nUnitType()
    var get_PhysicsAttack = get_func_by_offset("libil2cpp.so",0x3CF9A8F); //[Address(RVA="0x3CF9A8F", Offset="0x3CF9A8F")]public int get_PhysicsAttack()
    var get_MagicalAttack = get_func_by_offset("libil2cpp.so",0x3CF9ABA);  //[Address(RVA="0x3CF9ABA", Offset="0x3CF9ABA")]public int get_MagicalAttack()
    var get_MagicalDefence = get_func_by_offset("libil2cpp.so",0x3CF9B10); //[Address(RVA="0x3CF9B10", Offset="0x3CF9B10")]public int get_MagicalDefence()
    var get_PhysicsDefence = get_func_by_offset("libil2cpp.so",0x3CF9AE5); //[Address(RVA="0x3CF9AE5", Offset="0x3CF9AE5")] public int get_PhysicsDefence()
    var get_WalkSpeed = get_func_by_offset("libil2cpp.so",0x3CF96EC);//[Address(RVA="0x3CF96EC", Offset="0x3CF96EC")] public float get_WalkSpeed()
    hookmethod(get_nUnitType,"get_nUnitType");
    hookmethod1(get_PhysicsAttack,"get_PhysicsAttack");
    hookmethod1(get_MagicalAttack,"get_MagicalAttack");
    hookmethod1(get_MagicalDefence,"get_MagicalDefence");
    hookmethod1(get_PhysicsDefence,"get_PhysicsDefence");
    hookmethod1(get_WalkSpeed,"get_WalkSpeed");
}
var nUnitType;
function hookmethod(addr,str) {
    console.log("hook the " + str  + "  "+"the addr is" + addr.toString())
        Interceptor.attach(addr, {
        onEnter: function (args) {
        },
        onLeave: function(retval){
            nUnitType = retval.toInt32();    //读取nUnitType的值
        }
});
}
function hookmethod1(addr,str) {
            Interceptor.attach(addr, {
        onEnter: function (args) {
        },
        onLeave: function(retval){
            var value = retval.toInt32();   //读取返回值
            if (nUnitType == 1){     //如果nUnitType的值是1，即玩家，进入修改方法
                if (str.indexOf("get_WalkSpeed")!==-1){ //移动速度，似乎没效果，或者float数值写入错误
                    retval.replace(value * 3);
                }else {
                    retval.replace(value * 100000); //修改返回值*  100000
                }
                console.log("[+] " + str + "   " +  value.toString() + "->" + retval.toInt32().toString())  //打印结果
            }
        }

});
}




});

