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
    -- This is the hard-coded addr that our patched GameManager will try to mmap.
    -- It SHOULD work based on some statistics I did on HK's memory maps over a
    -- bunch of restarts, but if the game is being super unstable or whatever, maybe
    -- restart it.
    local tasInfoMap = 0x7f0000001000
    mapMarker = memory.readu64(tasInfoMap)
    if mapMarker == 0x1234567812345678 then
        tasInfoMarkerAddress = memory.readu64(tasInfoMap + 8)
        if memory.readu64(tasInfoMarkerAddress) == 1234567890123456789 then
            return memory.readu64(tasInfoMarkerAddress + 8)
        end
    end

    return 0;
end