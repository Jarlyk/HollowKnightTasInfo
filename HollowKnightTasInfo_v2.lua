function onPaint()
    local infoAddress = getInfoAddress()
	local screenWidth, screenHeight = gui.resolution()
	
    if infoAddress == 0 then
        gui.text(screenWidth - 5, 5, "info not found", 0xffffffff, 1, 0, 16, 1)
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
                gui.ellipse(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 2], 1, hitboxData[i + 3])
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
        gui.text(screenWidth - 5, 5 + 23 * (i - 1), v, 0xffffffff, 1, 0, 16, 1)
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
    tasFlags = 0
    mapMarker = memory.readu64(tasInfoMap)
    if mapMarker == 0x1234567812345678 then
        tasInfoMarkerAddress = memory.readu64(tasInfoMap + 8)
        if memory.readu64(tasInfoMarkerAddress) == 1234567890123456789 then
			tasFlags = memory.readu32(tasInfoMarkerAddress + 16)
            return memory.readu64(tasInfoMarkerAddress + 8)
        end
    end

    return 0;
end

function onStartup()
	tasFlags = 0
	wasFF = false
	unsafeStarted = false
end

function onFrame()
    local infoAddress = getInfoAddress()
	
	if not (tasFlags & 1 == 0) then
		-- Observed the start of unsafe FF zone
		print("start unsafe zone")
		wasFF = runtime.isFastForward()
		if wasFF then
			runtime.setFastForward(0)
		end
		unsafeStarted = true
	elseif not (tasFlags & 2 == 0) then
		--Observed the end of unsafe FF zone
		print("end unsafe zone")
		if wasFF then
			runtime.setFastForward(1)
		end
		unsafeStarted = false
	elseif not (tasFlags & 4 == 0) then
		--Currently in an unsafe FF zone
		local isFF = runtime.isFastForward()
		if isFF and not unsafeStarted then
            print("mid unsafe zone")
			wasFF = true
			runtime.setFastForward(0)
			unsafeStarted = true
		end
	end	
end
