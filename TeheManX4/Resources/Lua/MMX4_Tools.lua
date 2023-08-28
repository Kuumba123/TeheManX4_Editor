--========================================================
--			LUA SCRIPT for MMX4 Various Things [Redux]	--
--          made by PogChampGuy aka Kuumba              --
--========================================================

mmx4 = {
	--Redux Variables
	mem = PCSX.getMemPtr(),
	cache = PCSX.getScratchPtr(),
	scaleX = 0,
	scaleY = 0,
	megaX = 0,
	megaY = 0,
	--Cam Vars
	camX = 0,
	camY = 0,
	bg2X = 0,
	bg2Y = 0,
	bg3X = 0,
	bg3Y = 0,
	layoutW = 0,
	layoutH = 0,
	--Constants
	GAME_ADDR = 0x1721c0, --General Game Vars
	MEGA_ADDR = 0x1418c8,
	BG_ADDR = 0x1419b0,
	LAYOUT_ADDR = 0x141be8,
	LAYOUT_W_ADDR = 0x172224,
	LAYOUT_H_ADDR = 0x173a28,
	BORDER_TRIGGER_TABLE_ADDR = 0x10ae0c,
	--Object Constatns
	EFFECT_OBJ_ADDR = 0x142f98,
	--Collision Checkboxes
	showCollision = false,
	--Object Checkboxes
	showWepObj = true,
	showMainObj = true,
	showShotObj = true,
	showVisObj = true,
	showEffectObj = true,
	showItemObj = true,
	showMiscObj = true,
	showQuadObj = true,
	showLayerObj = true,
	showTrigger = true,
	--Raido Box for Id,Var,Slot
	objectOption = 1
}

function mmx4:AssignVariables()
    mmx4.megaX = bit.band(ffi.cast('short*', (mmx4.mem + mmx4.MEGA_ADDR + 10))[0],0xFFFF)
    mmx4.megaY = bit.band(ffi.cast('short*', mmx4.mem + mmx4.MEGA_ADDR + 14)[0],0xFFFF)

	mmx4.camX = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 10)[0],0xFFFF)
	mmx4.camY = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 14)[0],0xFFFF)

	mmx4.bg2X = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 10 + 84)[0],0xFFFF)
	mmx4.bg2Y = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 14 + 84)[0],0xFFFF)

	mmx4.bg3X = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 10 + 84 * 2)[0],0xFFFF)
	mmx4.bg3Y = bit.band(ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 14 + 84 * 2)[0],0xFFFF)

	mmx4.layoutW = ffi.cast("uint8_t*", mmx4.mem + mmx4.LAYOUT_W_ADDR)[0]
	mmx4.layoutH = ffi.cast("uint8_t*", mmx4.mem + mmx4.LAYOUT_H_ADDR)[0]
end

function mmx4:DrawCollision(winX,winY)

	for y = 0, 16 do
		for x = 0, 19 do
			local layoutOffset = bit.rshift((mmx4.camX + x * 16),8) + bit.rshift((mmx4.camY + y * 16),8) * mmx4.layoutW + mmx4.LAYOUT_ADDR

			local screenId = mmx4.mem[layoutOffset]

			local screenDataP = bit.band((ffi.cast("uint32_t*", mmx4.cache + 8))[0],0x1fffff)
			local tileInfoP = bit.band((ffi.cast("uint32_t*", mmx4.cache + 0xC))[0],0x1fffff)

			local tileId = bit.band(ffi.cast("short*",mmx4.mem + screenDataP + screenId * 0x200)[bit.rshift(bit.band(mmx4.camX + x * 16, 0xF0),4) + bit.band(mmx4.camY + y * 16,0xF0)],0x3FFF)

			local collisionId = bit.band(ffi.cast("uint8_t*",mmx4.mem + tileInfoP + tileId * 4)[0],0xFF)

			--Solid Tile
			if collisionId == 0x38 then
				local drawX = (x * 16 - bit.band(mmx4.camX,0xF)) * mmx4.scaleX + winX
				local drawY = (y * 16 - bit.band(mmx4.camY,0xF)) * mmx4.scaleY + winY

				nvg:beginPath()
				nvg:rect(drawX, drawY, 16 * mmx4.scaleX, 16 * mmx4.scaleY)
				nvg:strokeColor(nvg.RGBA(0, 0, 255, 128))
				nvg:strokeWidth(2)
				nvg:stroke()
			elseif collisionId == 0x39 then
				local drawX = (x * 16 - bit.band(mmx4.camX,0xF)) * mmx4.scaleX + winX
				local drawY = (y * 16 - bit.band(mmx4.camY,0xF)) * mmx4.scaleY + winY

				nvg:beginPath()
				nvg:rect(drawX, drawY, 16 * mmx4.scaleX, 16 * mmx4.scaleY)
				nvg:strokeColor(nvg.RGBA(0, 255, 0, 128))
				nvg:strokeWidth(2)
				nvg:stroke()
			elseif collisionId == 0x3C then
				local drawX = (x * 16 - bit.band(mmx4.camX,0xF)) * mmx4.scaleX + winX
				local drawY = (y * 16 - bit.band(mmx4.camY,0xF)) * mmx4.scaleY + winY

				nvg:beginPath()
				nvg:rect(drawX, drawY, 16 * mmx4.scaleX, 16 * mmx4.scaleY)
				nvg:strokeColor(nvg.RGBA(0, 191, 255, 128))
				nvg:strokeWidth(2)
				nvg:stroke()				
			elseif collisionId == 0x3E or collisionId == 0x3F then
				local drawX = (x * 16 - bit.band(mmx4.camX,0xF)) * mmx4.scaleX + winX
				local drawY = (y * 16 - bit.band(mmx4.camY,0xF)) * mmx4.scaleY + winY

				nvg:beginPath()
				nvg:rect(drawX, drawY, 16 * mmx4.scaleX, 16 * mmx4.scaleY)
				nvg:strokeColor(nvg.RGBA(255, 0, 0, 128))
				nvg:strokeWidth(2)
				nvg:stroke()				
			end
		end
	end
end

function mmx4:CheckObjectMem(addr, slots, size,winX,winY,r , g, b)
    for i = 0, slots do
        local alive = ffi.cast("uint8_t*", mmx4.mem + (i * size) + addr)[0]
        if alive ~= 0 then
            local id = ffi.cast('uint8_t*', mmx4.mem + (i * size) + addr + 1)[0]
            local objX = ffi.cast('short*', mmx4.mem + (i * size) + addr + 10)[0]
            local objY = ffi.cast('short*', mmx4.mem + (i * size) + addr + 14)[0]

			if mmx4.objectOption == 2 then
				id = ffi.cast('uint8_t*', mmx4.mem + (i * size) + addr + 2)[0]
			elseif mmx4.objectOption == 3 then
				id = i
			end
            local s = string.upper(string.format("%02x", id))
            local adjustedX = winX + ((objX - mmx4.camX) * mmx4.scaleX)
            local adjustedY = winY + ((objY - mmx4.camY) * mmx4.scaleY)

			local layer = ffi.cast('uint8_t*', mmx4.mem + (i * size) + addr + 0x14)[0]

			if layer == 1 then
				adjustedX = winX + ((objX - mmx4.bg2X) * mmx4.scaleX)
				adjustedY = winY + ((objY - mmx4.bg2Y) * mmx4.scaleY)
			elseif layer == 2 then
				adjustedX = winX + ((objX - mmx4.bg3X) * mmx4.scaleX)
				adjustedY = winY + ((objY - mmx4.bg3Y) * mmx4.scaleY)
			elseif layer ~= 0 then
				adjustedX = winX + ((objX - 0) * mmx4.scaleX)
				adjustedY = winY + ((objY - 0) * mmx4.scaleY)
			end

            local adjustedW = 22 * mmx4.scaleX
            local adjustedH = 22 * mmx4.scaleY

            -- Draw Rectangle
            nvg:beginPath()
            nvg:rect(adjustedX, adjustedY - adjustedH, adjustedW, adjustedH)
            nvg:fillColor(nvg.RGBA(r, g, b, 96))
            nvg:fill()

            -- Draw Text
            nvg:beginPath()
			nvg:fillColor(nvg.RGBA(255, 255, 255, 255))
			nvg:fontSize(20 * mmx4.scaleX)
            nvg:text(adjustedX, adjustedY, s)

            nvg:fill()
        end
    end
end
function mmx4:BorderObjectCheck(winX,winY)
	for i = 0, 0x1F do
		local alive = ffi.cast("uint8_t*", mmx4.mem + (i * 0x30) + mmx4.EFFECT_OBJ_ADDR)[0]
		if alive ~= 0 then
            local objId = ffi.cast('uint8_t*', mmx4.mem + (i * 0x30) + mmx4.EFFECT_OBJ_ADDR + 1)[0]
			if objId == 0 then
				local objX = ffi.cast('short*', mmx4.mem + (i * 0x30) + mmx4.EFFECT_OBJ_ADDR + 10)[0]
				local objY = ffi.cast('short*', mmx4.mem + (i * 0x30) + mmx4.EFFECT_OBJ_ADDR + 14)[0]
				local var = ffi.cast('uint8_t*', mmx4.mem + (i * 0x30) + mmx4.EFFECT_OBJ_ADDR + 2)[0]
				local id = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[12]
				local mid = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[13]
				if (id * 2 + mid) > 25 then
					return
				end
				
				local tableP = bit.band((ffi.cast("uint32_t*", mmx4.mem + mmx4.BORDER_TRIGGER_TABLE_ADDR))[id * 2 + mid], 0x1fffff)
				if tableP == 0 then
					return
				end
				local triggerP = bit.band((ffi.cast("uint32_t*", mmx4.mem + tableP))[var], 0x1fffff)
				
				--Get Trigger Sides
				local right = bit.band((ffi.cast("short*", mmx4.mem + triggerP))[0], 0xFFFF)
				local left = bit.band((ffi.cast("short*", mmx4.mem + triggerP))[1], 0xFFFF)
				local bottom = bit.band((ffi.cast("short*", mmx4.mem + triggerP))[2], 0xFFFF)
				local top = bit.band((ffi.cast("short*", mmx4.mem + triggerP))[3], 0xFFFF)

				local width = (right - left) * mmx4.scaleX
				local height = (bottom - top) * mmx4.scaleY
				local drawX = (left - mmx4.camX) * mmx4.scaleX + winX
				local drawY = (top - mmx4.camY) * mmx4.scaleY + winY

				-- Draw Rectangle
				nvg:beginPath()
				nvg:rect(drawX, drawY, width, height)
				nvg:fillColor(nvg.RGBA(0xAD, 0xD8, 0xE6, 96))
				nvg:strokeColor(nvg.RGBA(0, 255, 0, 128))
				nvg:strokeWidth(2)
				nvg:stroke()
				nvg:fill()
			end
		end
	end
end

function mmx4:DrawObjectControls()
    local isHeaderOpen = imgui.CollapsingHeader("Object Settings")
    if isHeaderOpen then
        local changed, value = imgui.Checkbox("Weapon Objects", mmx4.showWepObj)
        if changed then
            mmx4.showWepObj = value
        end
        changed, value = imgui.Checkbox("Main Objects", mmx4.showMainObj)
        if changed then
            mmx4.showMainObj = value
        end
        changed, value = imgui.Checkbox("Shot Objects", mmx4.showShotObj)
        if changed then
            mmx4.showShotObj = value
        end
        changed, value = imgui.Checkbox("Visual Objects", mmx4.showVisObj)
        if changed then
            mmx4.showVisObj = value
        end
        changed, value = imgui.Checkbox("Effect Objects", mmx4.showEffectObj)
        if changed then
            mmx4.showEffectObj = value
        end
        changed, value = imgui.Checkbox("Item Objects", mmx4.showItemObj)
        if changed then
            mmx4.showItemObj = value
        end
        changed, value = imgui.Checkbox("Misc Objects", mmx4.showMiscObj)
        if changed then
            mmx4.showMiscObj = value
        end
        changed, value = imgui.Checkbox("Quad Objects", mmx4.showQuadObj)
        if changed then
            mmx4.showQuadObj = value
        end
        changed, value = imgui.Checkbox("Layer Objects", mmx4.showLayerObj)
        if changed then
            mmx4.showLayerObj = value
        end

        -- Create radio buttons for Id/Var/Slot
        if imgui.RadioButton("Id", mmx4.objectOption == 1) then
            mmx4.objectOption = 1
        end
        imgui.SameLine()
        if imgui.RadioButton("Var", mmx4.objectOption == 2) then
            mmx4.objectOption = 2
        end
        imgui.SameLine()
        if imgui.RadioButton("Slot", mmx4.objectOption == 3) then
            mmx4.objectOption = 3
        end

		changed, value = imgui.Checkbox("Camera Triggers", mmx4.showTrigger)
        if changed then
            mmx4.showTrigger = value
        end
    end
end

val = 0
step = 1

function mmx4:DrawGeneral()
    local isHeaderOpen = imgui.CollapsingHeader("General")
    if isHeaderOpen then
        local mode = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[0]
        local mode2 = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[1]
        local mode3 = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[2]
        local id = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[12]
        local mid = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[13]
        local checkpoint = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[29]
        local enableBoss = ffi.cast("uint8_t*", mmx4.mem + mmx4.GAME_ADDR)[36]

        -- Print Mode
        imgui.TextUnformatted("Mode: " .. string.format("%X", mode) .. "-" .. mode2 .. "-" .. mode3)
        if mode == 1 then
            imgui.SameLine()
            imgui.TextUnformatted(" (Player Select)")
        elseif mode == 3 then
            imgui.SameLine()
            imgui.TextUnformatted(" (Stage Select)")
        elseif mode == 6 then
            imgui.SameLine()
            imgui.TextUnformatted(" (Main)")
        elseif mode == 7 then
            imgui.SameLine()
            imgui.TextUnformatted(" (Weapon Get)")
        end

        -- Print current Stage
        imgui.TextUnformatted("STAGE: " .. string.format("%X", id) .. "-" .. mid)

		imgui.SameLine()
		if imgui.Button("Clear") then
			mmx4.mem[0x1721cf] = 1
		end

        -- Boss Stuff
        if enableBoss ~= 0 then
            local bossP = bit.band((ffi.cast("uint32_t*", mmx4.mem + mmx4.GAME_ADDR + 32))[0], 0x1fffff)
            local hp = mmx4.mem[bossP + 92]
            imgui.TextUnformatted("Boss HP: " .. hp)
        end

        -- Checkpoint Edit
        imgui.TextUnformatted("Checkpoint: " .. string.format("%X", checkpoint))
    end
end


function mmx4:DrawBackgroundHeaders()
    local isHeaderOpen = imgui.CollapsingHeader("Background 1 Settings")
    if isHeaderOpen then
        imgui.TextUnformatted("X:" .. string.upper(string.format("%04x", mmx4.camX)))
        imgui.TextUnformatted("Y:" .. string.upper(string.format("%04x", mmx4.camY)))
        imgui.TextUnformatted("Enabled: ")
        imgui.SameLine()
        if mmx4.mem[mmx4.BG_ADDR + 3] == 0 then
            imgui.TextUnformatted("FALSE")
        else
            imgui.TextUnformatted("TRUE")
        end

        imgui.TextUnformatted("Scroll Type: " .. mmx4.mem[mmx4.BG_ADDR + 4])

        imgui.TextUnformatted("Border-Right: " .. string.upper(string.format("%04x", ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 36)[0])))
        imgui.TextUnformatted("Border-Left: " .. string.upper(string.format("%04x", ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 36)[1])))
        imgui.TextUnformatted("Border-Bottom: " .. string.upper(string.format("%04x", ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 36)[2])))
        imgui.TextUnformatted("Border-Top: " .. string.upper(string.format("%04x", ffi.cast('short*', mmx4.mem + mmx4.BG_ADDR + 36)[3])))
        local changed, value = imgui.Checkbox("Show Collision", mmx4.showCollision)
        if changed then
            mmx4.showCollision = value
        end
    end

    isHeaderOpen = imgui.CollapsingHeader("Background 2 Settings")
    if isHeaderOpen then
        imgui.TextUnformatted("X:" .. string.upper(string.format("%04x", mmx4.bg2X)))
        imgui.TextUnformatted("Y:" .. string.upper(string.format("%04x", mmx4.bg2Y)))
        imgui.TextUnformatted("Enabled: ")
        imgui.SameLine()
        if mmx4.mem[mmx4.BG_ADDR + 3 + 84] == 0 then
            imgui.TextUnformatted("FALSE")
        else
            imgui.TextUnformatted("TRUE")
        end

        imgui.TextUnformatted("Scroll Type: " .. mmx4.mem[mmx4.BG_ADDR + 4 + 84])
    end

    isHeaderOpen = imgui.CollapsingHeader("Background 3 Settings")
    if isHeaderOpen then
        imgui.TextUnformatted("X:" .. string.upper(string.format("%04x", mmx4.bg3X)))
        imgui.TextUnformatted("Y:" .. string.upper(string.format("%04x", mmx4.bg3Y)))
        imgui.TextUnformatted("Enabled: ")
        imgui.SameLine()
        if mmx4.mem[mmx4.BG_ADDR + 3 + 84 * 2] == 0 then
            imgui.TextUnformatted("FALSE")
        else
            imgui.TextUnformatted("TRUE")
        end

        imgui.TextUnformatted("Scroll Type: " .. mmx4.mem[mmx4.BG_ADDR + 4 + 84 * 2])
    end
end

function mmx4:DrawPlayerState(player,status,state,sub)
	imgui.TextUnformatted("Status: "..status)

	imgui.TextUnformatted("State: "..string.format("%X",state).."-"..string.format("%X",sub))

	if state == 2 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Standing)")
	elseif state == 3 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Start Walking)")
	elseif state == 4 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Walking)")
	elseif state == 6 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Jumping)")
	elseif state == 7 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Falling)")
	elseif state == 9 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Clinged on Wall)")
	elseif state == 0xA then
		imgui.SameLine()
		imgui.TextUnformatted(" (Wall Jumping)")
	elseif state == 0xB then
		imgui.SameLine()
		imgui.TextUnformatted(" (Wall Sliding)")
	elseif state == 0xC then
		imgui.SameLine()
		imgui.TextUnformatted(" (Dashing)")
	elseif state == 0xD then
		imgui.SameLine()
		imgui.TextUnformatted(" (Air-Dashing)")
	elseif state == 0xE then
		imgui.SameLine()
		imgui.TextUnformatted(" (Getting off/on Ladder)")
	elseif state == 0xF then
		imgui.SameLine()
		imgui.TextUnformatted(" (Going Up Ladder)")
	elseif state == 0x10 then
		imgui.SameLine()
		imgui.TextUnformatted(" (Going Down Ladder)")
	end
end

function mmx4:DrawPlayerInfo()
	local player = mmx4.mem[mmx4.MEGA_ADDR + 2]
	local status = mmx4.mem[mmx4.MEGA_ADDR + 4]
	local state = mmx4.mem[mmx4.MEGA_ADDR + 5]
	local sub = mmx4.mem[mmx4.MEGA_ADDR + 6]
	local hp = mmx4.mem[mmx4.MEGA_ADDR + 92]
	local maxHP = mmx4.mem[mmx4.MEGA_ADDR + 94]
	if player == 0 then	--is X
		local isHeaderOpen = imgui.CollapsingHeader("MegaMan Settings")
		if isHeaderOpen then
			local weapon = mmx4.mem[mmx4.MEGA_ADDR + 147]
			imgui.TextUnformatted("X:" .. string.upper(string.format("%04x", mmx4.megaX)))
			imgui.TextUnformatted("Y:" .. string.upper(string.format("%04x", mmx4.megaY)))
			mmx4:DrawPlayerState(player,status,state,sub)
			imgui.TextUnformatted("Weapon: " .. string.format("%01X", weapon))
			imgui.TextUnformatted("HP: " .. hp .. "  Max HP: ".. maxHP);
			imgui.SameLine()
			if imgui.Button("Fill") then
				mmx4.mem[mmx4.MEGA_ADDR + 92] = maxHP
			end
		end
	else --Is Zero
		local isHeaderOpen = imgui.CollapsingHeader("Zero Settings")
		if isHeaderOpen then
			imgui.TextUnformatted("X:" .. string.upper(string.format("%04x", mmx4.megaX)))
			imgui.TextUnformatted("Y:" .. string.upper(string.format("%04x", mmx4.megaY)))
			mmx4:DrawPlayerState(player,status,state,sub)

			imgui.TextUnformatted("HP: " .. hp .. "  Max HP: ".. maxHP);
			imgui.SameLine()
			if imgui.Button("Fill") then
				mmx4.mem[mmx4.MEGA_ADDR + 92] = maxHP
			end
		end	
	end
end



function DrawImguiFrame()

	imgui.Begin('MegaMan X4 Tools', false)

	mmx4:AssignVariables()
	mmx4:DrawGeneral()
	mmx4:DrawPlayerInfo()
	mmx4:DrawBackgroundHeaders()
	mmx4:DrawObjectControls()
	imgui.End()

end

function mmx4:unload()
	DrawImguiFrame = nil
	PCSX.GUI.OutputShader.setDefaults()
	for key, _ in pairs(mmx4) do
		mmx4[key] = nil
	end
	mmx4.unload = nil
end

--New Contents of output.lua
PCSX.GUI.OutputShader.setTextL([[function Image(textureID, srcSizeX, srcSizeY, dstSizeX, dstSizeY)
	local winX, winY = PCSX.Helpers.UI.imageCoordinates(0, 0, 1.0, 1.0, dstSizeX, dstSizeY)

	-- Calculate the scaling factor based on the desired width (e.g., 320 pixels)
	mmx4.scaleX = dstSizeX / 320
	mmx4.scaleY = dstSizeY / 240
	nvg:queueNvgRender(function()
		if mmx4.showCollision then
			mmx4:DrawCollision(winX,winY)
		end
		if mmx4.showTrigger then
			mmx4:BorderObjectCheck(winX,winY)
		end

		if mmx4.showWepObj then
			mmx4:CheckObjectMem(0x1406f8,0x10 - 1,0x9C,winX,winY,0,0xFF,0) --Weapon
		end
		if mmx4.showMainObj then
			mmx4:CheckObjectMem(0x13bed0,0x30 - 1,0x9C,winX,winY,0xFF,0,0) --Main
		end
		if mmx4.showShotObj then
			mmx4:CheckObjectMem(0x13f328,0x20 - 1,0x9C,winX,winY,0xCF,0x52,0x0E) --Shot
		end
		if mmx4.showVisObj then
			mmx4:CheckObjectMem(0x13e510,0x20 - 1,0x70,winX,winY,0,0xFF,0xFF) --Visual
		end
		if mmx4.showEffectObj then
			mmx4:CheckObjectMem(mmx4.EFFECT_OBJ_ADDR,0x20 - 1,0x30,winX,winY,0xFF,0x14,0x93) --Effect
		end
		if mmx4.showItemObj then
			mmx4:CheckObjectMem(0x165a30,0x20 - 1,0x8C,winX,winY,0,0,0xFF) --Item
		end
		if mmx4.showMiscObj then
			mmx4:CheckObjectMem(0x173ca0,0x40 - 1,0x60,winX,winY,0x8A,0x2B,0xE2) --Misc
		end
		if mmx4.showQuadObj then
			mmx4:CheckObjectMem(0x1435b0, 0x20 - 1, 0x60,winX,winY,0xFF,0xFF,00) --Quad
		end
		if mmx4.showLayerObj then
			mmx4:CheckObjectMem(0x169cb8,4 - 1,0x30,winX,winY,0xFF,0xFF,0xFF) --Layer
		end
	end)

	imgui.Image(textureID, dstSizeX, dstSizeY, 0, 0, 1, 1)
end]])