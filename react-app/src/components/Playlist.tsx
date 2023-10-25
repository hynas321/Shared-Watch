import { BsPlusCircleFill, BsXCircle } from "react-icons/bs";
import { QueuedVideo } from "../types/QueuedVideo";
import Button from "./Button";
import { TextInput } from "./TextInput";
import { useEffect, useRef, useState } from "react";

export default function Playlist() {
  const [queuedVideos, setQueuedVideos] = useState<QueuedVideo[]>([]);
  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");
  const queuedVideosRef = useRef<HTMLDivElement>(null);

  const videoThumbnailStyle = {
    width: "60px",
    height: "45px"
  };

  useEffect(() => {
    if (queuedVideosRef.current) {
      queuedVideosRef.current.scrollTop = queuedVideosRef.current.scrollHeight;
    }
  }, [queuedVideos]);

  const handleTextInputChange = (text: string) => {
    setCurrentVideoUrlText(text);
  }

  const handleSendVideoUrl = () => {
    if (!currentVideoUrlText || currentVideoUrlText?.length === 0) {
      return;
    }

    const newQueuedVideo: QueuedVideo = {
      url: "https://www.youtube.com/watch?v=p2zMXSXhZ9M",
      image: "https://img.youtube.com/vi/UgkvcWx8oXw/2.jpg",
      title: "Interstellar | Melancholic Melody, 1 Hour Magical Journey, Sleep Aid, Ambient Music"
      //title can be fetched from webpage's title (from URL)
    };

    setQueuedVideos([...queuedVideos, newQueuedVideo])
    setCurrentVideoUrlText("");
  };

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleSendVideoUrl();
    }
  }

  return (
    <>
      <div className="d-flex mb-3">
        <TextInput
          value={currentVideoUrlText}
          placeholder="Paste video URL"
          onChange={handleTextInputChange}
          onKeyDown={handleEnterPress}
        />
        <Button
          text={<BsPlusCircleFill />}
          classNames="btn btn-primary"
          onClick={handleSendVideoUrl}
        />
      </div>
      <div className="list-group rounded-3 control-panel-list" ref={queuedVideosRef}>
      {
        queuedVideos.length !== 0 ? (
          queuedVideos.map((queuedVideo, index) => (
<a 
  key={index}
  className="border border-secondary list-group-item bg-muted border-2 a-video"
  href={queuedVideo.url}
>
  <div className="row">
    <div className="col-auto">
        {queuedVideo.image !== undefined && <img src={queuedVideo.image} style={videoThumbnailStyle} alt="Video Thumbnail" />}
    </div>
    <div className="d-flex col text-secondary align-items-center">
      <small>{queuedVideo.title !== undefined ? queuedVideo.title : queuedVideo.url}</small>
      <div>
        <Button
          text={<BsXCircle/>}
          classNames="btn btn-outline-danger btn-sm"
          styles={{marginLeft: "5px", }}
          onClick={() => {}}
        />
      </div>
    </div>
  </div>
</a>
          ))
        ) :
        <h6 className="text-white text-center">No videos to display</h6>
      }
      </div>
    </>
  )
}
