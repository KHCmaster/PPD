PPDPACKV1 Mod\DisplayName
   Mod\AuthorName	   Mod\Version   Mod\FlowScriptVersion   Mod\FlowScriptBasicVersion   Mod\FlowScriptPPDVersion   Mod\FlowScriptSlimDXVersion   Scripts\GameE  Scripts\Mod�  Image\black.png�  BlackMovieKHCmaster1.2.0.01.9.0.01.9.0.01.9.0.01.9.0.0<Root>
  <Sources>
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Graphics.StageObjectFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Graphics.StageObjectFlowSourceObject" ID="3" ShowAll="True" X="-310" Y="-90">
      <Value Name="Layer" Value="AfterMovie" />
    </Source>
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Graphics.Image.ValueFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Graphics.Image.ValueFlowSourceObject" ID="4" ShowAll="True" X="-150" Y="-230">
      <Value Name="Path" Value="black.png" />
    </Source>
    <Source Name="FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject" FullName="FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject" ID="1" ShowAll="True" X="-340" Y="-300" />
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Graphics.AddChildFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Graphics.AddChildFlowSourceObject" ID="2" ShowAll="True" X="140" Y="-170" />
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Graphics.SetAlphaFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Graphics.SetAlphaFlowSourceObject" ID="5" ShowAll="True" X="160" Y="-340">
      <Value Name="Alpha" Value="0.5" />
    </Source>
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Mod.GetSettingFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Mod.GetSettingFlowSourceObject" ID="6" ShowAll="True" X="-199" Y="-390.5">
      <Value Name="Key" Value="Opacity" />
    </Source>
  </Sources>
  <Flows>
    <Flow SrcID="3" SrcName="Value" DestID="2" DestName="Parent" />
    <Flow SrcID="4" SrcName="Success" DestID="2" DestName="In" />
    <Flow SrcID="4" SrcName="Success" DestID="5" DestName="In" />
    <Flow SrcID="4" SrcName="Object" DestID="2" DestName="Child" />
    <Flow SrcID="4" SrcName="Object" DestID="5" DestName="Object" />
    <Flow SrcID="1" SrcName="Start" DestID="4" DestName="In" />
    <Flow SrcID="6" SrcName="Value" DestID="5" DestName="Alpha" />
  </Flows>
  <Comments />
  <Scopes />
</Root><Root>
  <Sources>
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Mod.AddFloatSettingWithLimitFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Mod.AddFloatSettingWithLimitFlowSourceObject" ID="7" ShowAll="True" X="-10" Y="-387.5">
      <Value Name="Default" Value="0.5" />
      <Value Name="Description" Value="黒さの度合いを設定します" />
      <Value Name="DisplayName" Value="黒さの度合い" />
      <Value Name="Key" Value="Opacity" />
      <Value Name="Max" Value="1" />
      <Value Name="Min" Value="0" />
    </Source>
    <Source Name="FlowScriptEnginePPD.FlowSourceObjects.Mod.EntryPointFlowSourceObject" FullName="FlowScriptEnginePPD.FlowSourceObjects.Mod.EntryPointFlowSourceObject" ID="1" ShowAll="True" X="-329" Y="-364.5" />
  </Sources>
  <Flows>
    <Flow SrcID="1" SrcName="Start" DestID="7" DestName="In" />
  </Flows>
  <Comments />
  <Scopes />
</Root>�PNG

   IHDR     �   �:�   tEXtSoftware Adobe ImageReadyq�e<  �IDATx��ֱ	  ðB���O�H'xr ��H  `�   �� �` ,  � �� 0X    � `�   �� �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,   �� 0X    � `�     �` ,   �� 0X  ,  � `�     �` ,   �� 0X  ,  � `�     �` ,   �� 0X  ,  � `�  0X   �` ,   �� 0X  ,  � `�  0X   �` `�   �� 0X  ,  � `�  0X   �` `�   �� �` ,  � `�  0X   �` `�   �� �` ,  � `�  0X   �` `�   �� �` ,  � �� 0X   �` `�   �� �` ,  � �� 0X    � `�   �� �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,   �� 0X    � `�     �` ,   �� 0X  ,  � `�     �` ,   �� 0X  ,  � `�  0X   �` ,   �� 0X  ,  � `�  0X   �` ,   �� 0X  ,  � `�  0X   �` `�   �� 0X  ,  � `�  0X   �` `�   �� �` ,  � `�  0X   �` `�   �� �` ,  � �� 0X   �` `�   �� �` ,  � �� 0X   �`I  `�   �� �` ,  � �� 0X    � `�   �� �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,   �� 0X    � `�     �` ,   �� 0X  ,  � `�     �` ,   �� 0X  ,  � `�     �` ,   �� 0X  ,  � `�  0X   �` ,   �� 0X  ,  � `�  0X   �` `�   �� 0X  ,  � `�  0X   �` `�   �� �` ,  � `�  0X   �` `�   �� �` ,  � `�  0X   �` `�   �� �` ,  � �� 0X   �` `�   �� �` ,  � �� 0X    � `�   �� �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,  � �� 0X    � `�     �` ,   �� 0X    � `�     �` ,   �� 0X  ,  � `�     �` ,   ��0 �G�㞱    IEND�B`�