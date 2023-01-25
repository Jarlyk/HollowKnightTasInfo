local gameVersion
local markAddresses = {
    v1028 = { 0x400000 + 0x1B1CF60, 0x400, 0x18, 0x20, 0xF78 },
    v1037 = { 0x400000 + 0x1B1CF60, 0x400, 0x18, 0x20, 0xF78 },
    v1221 = { 0x400000 + 0x1B1CF60, 0x400, 0x18, 0x20, 0xF38 },
    v1432_mod = { 0x400000 + 0x2121D18, 0x0, 0x40, 0x4A0, 0x48, 0xA0, 0x68 },
    v1432 = { 0x400000 + 0x21047C0, 0xF8, 0x80, 0x60, 0x20, 0xA0, 0x238, 0xF48}
}

function onPaint()
    local infoAddress = getInfoAddress()

    if infoAddress == 0 then
        gui.text(9999, 0, "info not found")
        return
    end

    local infoText = readString(infoAddress)
    local gameInfo = {}
    for line in infoText:gmatch("[^\r\n]+") do
        if line:find("^Enemy=") ~= nil then
            local hpData = splitString(line:sub(7), "|")
            for i = 1, #hpData, 3 do
                gui.text(hpData[i], hpData[i + 1], hpData[i + 2])
            end
        elseif line:find("^LineHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(12), "|")
            for i = 1, #hitboxData, 5 do
                gui.line(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxData[i + 4])
            end
        elseif line:find("^CircleHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(14), "|")
            for i = 1, #hitboxData, 4 do
                gui.ellipse(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 2], hitboxData[i + 3])
            end
        else
            table.insert(gameInfo, line)
        end
    end

    drawGameInfo(gameInfo)
end

function drawGameInfo(textArray)
    local screenWidth, screenHeight = gui.resolution()
    for i, v in ipairs(textArray) do
        gui.text(screenWidth, 23 * (i - 1), v)
    end
end

function readString(address)
    local text = {}
    local len = memory.readu16(address + 0x10)
    for i = 1, len do
        text[i] = string.char(memory.readu16(address + 0x12 + i * 2))
    end
    return table.concat(text)
end

function splitString(text, sep)
    if sep == nil then
        sep = "%s"
    end
    local t = {}
    for str in string.gmatch(text, "([^" .. sep .. "]+)") do
        table.insert(t, str)
    end
    return t
end

function getInfoAddress()
    if gameVersion == nil then
        for k, v in pairs(markAddresses) do
            if getPointerAddress(v) == 1234567890123456789 then
                gameVersion = k;
                v[#v] = v[#v] + 0x8
            end
        end
    end

    if gameVersion ~= nil then
        return getPointerAddress(markAddresses[gameVersion])
    end

    return 0;
end

function getPointerAddress(offsets)
    local address = 0
    for i, v in ipairs(offsets) do
        address = memory.readu64(address + v)
        if address == 0 then
            return 0
        end
    end
    return address
end