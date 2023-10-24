export interface VideoInfoProps {
  videoName: string;
}

export default function VideoInfo({videoName}: VideoInfoProps) {
  return (
    <div className="rounded-bottom-5 bg-dark pt-3 pb-3 px-3 text-center">
        <span className="text-white">{videoName}</span>
    </div>
  )
}
